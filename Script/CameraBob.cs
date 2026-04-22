using UnityEngine;

public class CameraBob : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCamera;

    [Header("Bobbing Settings")]
    [SerializeField] private float walkingBobbingSpeed = 14f;
    [SerializeField] private float runningBobbingSpeed = 20f;
    [SerializeField] private float bobbingAmount = 0.1f;
    [SerializeField] private float runBobbingMultiplier = 1.5f;

    [Header("Vertical Bobbing")]
    [SerializeField] private float verticalBobbingAmount = 0.05f;
    [SerializeField] private float verticalBobbingSpeed = 10f;

    [Header("Thresholds")]
    [SerializeField] private float walkSpeedThreshold = 0.1f;
    [SerializeField] private float runSpeedThreshold = 5f;

    private Vector3 originalCameraPosition;
    private float timer = 0f;
    private bool isRunning = false;
    private Vector3 currentVelocity;

    void Start()
    {
        if (playerCamera == null)
        {
            // Ищем камеру в детях или создаем ссылку на основную камеру
            playerCamera = GetComponentInChildren<Camera>().transform;
            if (playerCamera == null)
                playerCamera = Camera.main.transform;
        }

        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();

        // Сохраняем оригинальное положение камеры относительно игрока
        if (playerCamera != null)
            originalCameraPosition = playerCamera.localPosition;
    }

    void Update()
    {
        HandleCameraBobbing();
    }

    private void HandleCameraBobbing()
    {
        if (characterController == null || playerCamera == null) return;

        // Проверяем движение и нахождение на земле
        bool isGrounded = characterController.isGrounded;
        bool isMoving = characterController.velocity.magnitude > walkSpeedThreshold && isGrounded;
        bool isRunningNow = characterController.velocity.magnitude > runSpeedThreshold && isGrounded;

        if (isRunningNow != isRunning)
        {
            isRunning = isRunningNow;
        }

        if (isMoving)
        {
            // Выбираем параметры в зависимости от состояния
            float currentBobbingSpeed = isRunning ? runningBobbingSpeed : walkingBobbingSpeed;
            float currentBobbingAmount = isRunning ? bobbingAmount * runBobbingMultiplier : bobbingAmount;

            // Увеличиваем таймер
            timer += Time.deltaTime * currentBobbingSpeed;

            // Вычисляем боббинг по осям X и Y
            float horizontalBob = Mathf.Sin(timer) * currentBobbingAmount;
            float verticalBob = Mathf.Cos(timer * 2) * verticalBobbingAmount * (isRunning ? runBobbingMultiplier : 1f);

            // Создаем новую позицию с боббингом
            Vector3 targetPosition = originalCameraPosition + new Vector3(horizontalBob, verticalBob, 0f);

            // Плавно применяем новую позицию
            playerCamera.localPosition = Vector3.SmoothDamp(playerCamera.localPosition, targetPosition, ref currentVelocity, 0.1f);
        }
        else
        {
            // Плавно возвращаем камеру в исходное положение
            if (Vector3.Distance(playerCamera.localPosition, originalCameraPosition) > 0.001f)
            {
                playerCamera.localPosition = Vector3.SmoothDamp(playerCamera.localPosition, originalCameraPosition, ref currentVelocity, 0.1f);
            }
            else
            {
                // Сбрасываем таймер когда камера вернулась
                timer = 0f;
                playerCamera.localPosition = originalCameraPosition;
            }
        }
    }

    // Метод для принудительного сброса боббинга
    public void ResetBobbing()
    {
        timer = 0f;
        if (playerCamera != null)
            playerCamera.localPosition = originalCameraPosition;
    }

    // Метод для обновления оригинальной позиции (например, при приседании)
    public void UpdateOriginalPosition(Vector3 newPosition)
    {
        originalCameraPosition = newPosition;
    }
}