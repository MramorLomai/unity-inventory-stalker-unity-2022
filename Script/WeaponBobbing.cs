using UnityEngine;

public class WeaponBobbing : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform weaponTransform;

    [Header("Bobbing Settings")]
    [SerializeField] private float walkingBobbingSpeed = 14f;
    [SerializeField] private float runningBobbingSpeed = 20f;
    [SerializeField] private float bobbingRadius = 0.1f; // Радиус полукруга
    [SerializeField] private float runBobbingMultiplier = 1.5f;

    [Header("Speed Thresholds")]
    [SerializeField] private float walkSpeedThreshold = 0.1f;
    [SerializeField] private float runSpeedThreshold = 5f;

    [Header("Semi-Circle Settings")]
    [SerializeField] private float circleOffsetY = -0.05f; // Смещение вниз для полукруга
    [SerializeField] private bool invertDirection = false; // Инвертировать направление движения

    [Header("Smoothing")]
    [SerializeField] private float smoothTime = 0.1f;

    private Vector3 originalWeaponPosition;
    private float timer = 0f;
    private Vector3 currentVelocity;

    // Для плавного перехода между состояниями
    private float currentBobbingIntensity = 0f;
    private float targetBobbingIntensity = 0f;

    // Для хранения предыдущей позиции для плавности
    private Vector3 targetPosition;

    private void Start()
    {
        // Автоматическое получение ссылок если не установлены
        if (characterController == null)
            characterController = GetComponentInParent<CharacterController>();

        if (weaponTransform == null)
            weaponTransform = transform;

        originalWeaponPosition = weaponTransform.localPosition;
        targetPosition = originalWeaponPosition;
    }

    private void Update()
    {
        HandleWeaponBobbing();
    }

    private void HandleWeaponBobbing()
    {
        if (characterController == null) return;

        // Получаем текущую скорость персонажа
        Vector3 horizontalVelocity = new Vector3(
            characterController.velocity.x,
            0f,
            characterController.velocity.z
        );
        float currentSpeed = horizontalVelocity.magnitude;

        // Определяем интенсивность качания в зависимости от скорости
        targetBobbingIntensity = 0f;
        float currentBobbingSpeed = walkingBobbingSpeed;

        if (currentSpeed > walkSpeedThreshold)
        {
            if (currentSpeed > runSpeedThreshold)
            {
                // Бег - максимальная интенсивность
                targetBobbingIntensity = 1f * runBobbingMultiplier;
                currentBobbingSpeed = runningBobbingSpeed;
            }
            else
            {
                // Ходьба - пропорциональная интенсивность
                float walkIntensity = (currentSpeed - walkSpeedThreshold) /
                                    (runSpeedThreshold - walkSpeedThreshold);
                targetBobbingIntensity = Mathf.Clamp01(walkIntensity);
                currentBobbingSpeed = Mathf.Lerp(walkingBobbingSpeed, runningBobbingSpeed, walkIntensity);
            }
        }

        // Плавное изменение интенсивности
        currentBobbingIntensity = Mathf.Lerp(
            currentBobbingIntensity,
            targetBobbingIntensity,
            smoothTime * Time.deltaTime * 10f
        );

        // Если движемся - применяем качание по полукругу ЧЕРЕЗ НИЗ
        if (currentBobbingIntensity > 0.01f)
        {
            // Увеличиваем таймер на основе скорости
            timer += Time.deltaTime * currentBobbingSpeed;

            // Вычисляем угол для движения по полукругу через низ (от 180 до 360 градусов)
            float angle = Mathf.PingPong(timer, Mathf.PI) + Mathf.PI; // От PI до 2*PI (180 до 360 градусов)

            // Инвертируем направление если нужно
            if (invertDirection)
            {
                angle = (2 * Mathf.PI) - (angle - Mathf.PI); // Инвертируем диапазон
            }

            // Вычисляем позицию на полукруге (через низ)
            float x = Mathf.Cos(angle) * bobbingRadius * currentBobbingIntensity;
            float y = Mathf.Sin(angle) * bobbingRadius * currentBobbingIntensity + circleOffsetY;

            targetPosition = originalWeaponPosition + new Vector3(x, y, 0f);
        }
        else
        {
            // Возвращаем в исходное положение когда не движемся
            targetPosition = originalWeaponPosition;
            timer = 0f;
        }

        // Плавное перемещение оружия к целевой позиции
        weaponTransform.localPosition = Vector3.SmoothDamp(
            weaponTransform.localPosition,
            targetPosition,
            ref currentVelocity,
            smoothTime
        );
    }

    // Визуализация траектории полукруга в редакторе
    private void OnDrawGizmosSelected()
    {
        if (weaponTransform == null) return;

        Vector3 center = Application.isPlaying ?
            weaponTransform.parent.TransformPoint(originalWeaponPosition) :
            weaponTransform.position;

        // Рисуем полукруг ЧЕРЕЗ НИЗ (от 180 до 360 градусов)
        Gizmos.color = Color.cyan;
        int segments = 20;
        Vector3 previousPoint = center + new Vector3(
            Mathf.Cos(Mathf.PI) * bobbingRadius,
            Mathf.Sin(Mathf.PI) * bobbingRadius + circleOffsetY,
            0f
        );

        for (int i = 1; i <= segments; i++)
        {
            float angle = Mathf.PI + (i / (float)segments) * Mathf.PI; // От PI до 2*PI
            Vector3 point = center + new Vector3(
                Mathf.Cos(angle) * bobbingRadius,
                Mathf.Sin(angle) * bobbingRadius + circleOffsetY,
                0f
            );

            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // Рисуем исходную позицию
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(center, 0.02f);

        // Рисуем направление движения
        Gizmos.color = Color.yellow;
        Vector3 startPoint = center + new Vector3(
            Mathf.Cos(Mathf.PI) * bobbingRadius,
            Mathf.Sin(Mathf.PI) * bobbingRadius + circleOffsetY,
            0f
        );
        Vector3 midPoint = center + new Vector3(
            Mathf.Cos(1.5f * Mathf.PI) * bobbingRadius,
            Mathf.Sin(1.5f * Mathf.PI) * bobbingRadius + circleOffsetY,
            0f
        );
        Vector3 endPoint = center + new Vector3(
            Mathf.Cos(2 * Mathf.PI) * bobbingRadius,
            Mathf.Sin(2 * Mathf.PI) * bobbingRadius + circleOffsetY,
            0f
        );

        Gizmos.DrawLine(startPoint, midPoint);
        Gizmos.DrawLine(midPoint, endPoint);
    }

    // Метод для принудительного сброса позиции оружия
    public void ResetWeaponPosition()
    {
        weaponTransform.localPosition = originalWeaponPosition;
        targetPosition = originalWeaponPosition;
        timer = 0f;
        currentBobbingIntensity = 0f;
        targetBobbingIntensity = 0f;
    }

    // Методы для изменения параметров во время выполнения
    public void SetBobbingRadius(float radius)
    {
        bobbingRadius = radius;
    }

    public void SetCircleOffset(float offset)
    {
        circleOffsetY = offset;
    }

    public void SetBobbingSpeed(float walkSpeed, float runSpeed)
    {
        walkingBobbingSpeed = walkSpeed;
        runningBobbingSpeed = runSpeed;
    }

    public void SetInvertDirection(bool invert)
    {
        invertDirection = invert;
    }

    // Для отладки - показывает текущую интенсивность качания
    public float GetCurrentBobbingIntensity()
    {
        return currentBobbingIntensity;
    }
}