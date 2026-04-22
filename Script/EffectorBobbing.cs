using UnityEngine;

public class EffectorBobbing : MonoBehaviour
{
    [Header("Bobbing Settings")]
    [SerializeField] private float runAmplitude = 0.1f;
    [SerializeField] private float walkAmplitude = 0.05f;
    [SerializeField] private float limpAmplitude = 0.08f;

    [SerializeField] private float runSpeed = 10f;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float limpSpeed = 3f;

    [Header("Smoothing Settings")]
    [SerializeField] private float positionSmoothTime = 0.1f;
    [SerializeField] private float rotationSmoothTime = 0.15f;
    [SerializeField] private float stateTransitionTime = 0.2f;

    [Header("References")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private CharacterController characterController;

    private const float CrouchFactor = 0.75f;
    private const float SpeedReminder = 5f;

    private float fTime = 0f;
    private float fReminderFactor = 0f;
    private bool isLimping = false;
    private bool isZoomMode = false;

    private bool isMoving = false;
    private bool isCrouching = false;
    private bool isRunning = false;

    private Vector3 originalCameraPosition;
    private Quaternion originalCameraRotation;
    private Vector3 lastPosition;

    // Сглаживающие переменные
    private Vector3 currentBobbingPosition;
    private Vector3 targetBobbingPosition;
    private Vector3 positionSmoothVelocity;

    private Quaternion currentBobbingRotation;
    private Quaternion targetBobbingRotation;
    private float rotationSmoothVelocity;

    private float currentAmplitude;
    private float targetAmplitude;
    private float amplitudeSmoothVelocity;

    private float currentBobbingSpeed;
    private float targetBobbingSpeed;
    private float speedSmoothVelocity;

    // Переменные для плавных переходов
    private float currentCrouchFactor = 1f;
    private float crouchSmoothVelocity;

    private void Start()
    {
        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        originalCameraPosition = cameraTransform.localPosition;
        originalCameraRotation = cameraTransform.localRotation;
        lastPosition = transform.position;

        // Инициализация сглаживающих переменных
        currentBobbingPosition = originalCameraPosition;
        targetBobbingPosition = originalCameraPosition;
        currentBobbingRotation = originalCameraRotation;
        targetBobbingRotation = originalCameraRotation;
        currentAmplitude = 0f;
        targetAmplitude = 0f;
        currentBobbingSpeed = 0f;
        targetBobbingSpeed = 0f;
    }

    private void Update()
    {
        UpdateMovementState();
        ProcessBobbing();
        ApplySmoothBobbing();
    }

    private void UpdateMovementState()
    {
        Vector3 currentPosition = transform.position;
        float horizontalSpeed = Vector3.ProjectOnPlane(currentPosition - lastPosition, Vector3.up).magnitude / Time.deltaTime;
        lastPosition = currentPosition;

        bool wasMoving = isMoving;
        isMoving = horizontalSpeed > 0.1f && characterController.isGrounded;

        float runThreshold = 3f;
        isRunning = horizontalSpeed > runThreshold && !isZoomMode;

        // Плавное изменение фактора приседания
        float targetCrouchFactor = isCrouching ? CrouchFactor : 1f;
        currentCrouchFactor = Mathf.SmoothDamp(currentCrouchFactor, targetCrouchFactor, ref crouchSmoothVelocity, 0.15f);
    }

    public void SetState(bool crouching, bool limping, bool zoomMode)
    {
        isCrouching = crouching;
        isLimping = limping;
        isZoomMode = zoomMode;
    }

    private void ProcessBobbing()
    {
        fTime += Time.deltaTime;

        // Плавное обновление фактора напоминания
        float targetReminderFactor = isMoving ? 1f : 0f;
        fReminderFactor = Mathf.MoveTowards(fReminderFactor, targetReminderFactor, SpeedReminder * Time.deltaTime);

        if (!Mathf.Approximately(fReminderFactor, 0f))
        {
            // Плавное определение целевых параметров боббинга
            if (isRunning)
            {
                targetAmplitude = runAmplitude * currentCrouchFactor;
                targetBobbingSpeed = runSpeed * currentCrouchFactor;
            }
            else if (isLimping)
            {
                targetAmplitude = limpAmplitude * currentCrouchFactor;
                targetBobbingSpeed = limpSpeed * currentCrouchFactor;
            }
            else
            {
                targetAmplitude = walkAmplitude * currentCrouchFactor;
                targetBobbingSpeed = walkSpeed * currentCrouchFactor;
            }

            // Сглаживание амплитуды и скорости
            currentAmplitude = Mathf.SmoothDamp(currentAmplitude, targetAmplitude, ref amplitudeSmoothVelocity, stateTransitionTime);
            currentBobbingSpeed = Mathf.SmoothDamp(currentBobbingSpeed, targetBobbingSpeed, ref speedSmoothVelocity, stateTransitionTime);

            // Вычисление плавных волн боббинга
            float ST = currentBobbingSpeed * fTime;

            // Используем разные фазы для позиции и вращения для более естественного движения
            float verticalWave = Mathf.Sin(ST) * currentAmplitude * fReminderFactor;
            float horizontalWave = Mathf.Cos(ST * 0.5f) * currentAmplitude * 0.3f * fReminderFactor;
            float rollWave = Mathf.Cos(ST * 0.8f) * currentAmplitude * 0.4f * fReminderFactor;

            // Целевая позиция с боббингом
            targetBobbingPosition = originalCameraPosition + new Vector3(horizontalWave, verticalWave, 0f);

            // Целевое вращение с боббингом
            Vector3 eulerRotation = new Vector3(rollWave * 10f, 0f, rollWave * 15f);
            targetBobbingRotation = originalCameraRotation * Quaternion.Euler(eulerRotation);
        }
        else
        {
            // Плавный возврат к исходным значениям
            targetAmplitude = 0f;
            targetBobbingSpeed = 0f;
            currentAmplitude = Mathf.SmoothDamp(currentAmplitude, 0f, ref amplitudeSmoothVelocity, stateTransitionTime);

            targetBobbingPosition = originalCameraPosition;
            targetBobbingRotation = originalCameraRotation;

            // Сбрасываем время когда полностью остановились
            if (fReminderFactor <= 0.01f)
            {
                fTime = 0f;
            }
        }
    }

    private void ApplySmoothBobbing()
    {
        // Сглаженное применение позиции
        currentBobbingPosition = Vector3.SmoothDamp(
            currentBobbingPosition,
            targetBobbingPosition,
            ref positionSmoothVelocity,
            positionSmoothTime
        );

        cameraTransform.localPosition = currentBobbingPosition;

        // Сглаженное применение вращения
        float t = Mathf.SmoothDamp(0, 1, ref rotationSmoothVelocity, rotationSmoothTime);
        currentBobbingRotation = Quaternion.Slerp(
            currentBobbingRotation,
            targetBobbingRotation,
            t
        );

        cameraTransform.localRotation = currentBobbingRotation;
    }

    // Методы для внешнего доступа
    public float GetCurrentSpeed()
    {
        return characterController.velocity.magnitude;
    }

    public float GetHorizontalSpeed()
    {
        Vector3 horizontalVelocity = new Vector3(characterController.velocity.x, 0, characterController.velocity.z);
        return horizontalVelocity.magnitude;
    }

    public bool IsBobbingActive()
    {
        return fReminderFactor > 0.1f;
    }

    // Метод для принудительного сброса боббинга
    public void ResetBobbing()
    {
        fReminderFactor = 0f;
        fTime = 0f;
        targetBobbingPosition = originalCameraPosition;
        targetBobbingRotation = originalCameraRotation;
    }
}