using UnityEngine;

public class MouseLook : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 100f;
    public CharacterController playerController;

    [Header("Rotation Limits")]
    public float minVerticalAngle = -90f;
    public float maxVerticalAngle = 90f;

    private float xRotation = 0f;

    void Start()
    {
        // Заблокировать и скрыть курсор
        Cursor.lockState = CursorLockMode.Locked;

        // Автоматически найти CharacterController если не назначен
        if (playerController == null)
        {
            playerController = GetComponentInParent<CharacterController>();
        }
    }

    void Update()
    {
        HandleMouseLook();
    }

    void HandleMouseLook()
    {
        // Получаем ввод мыши
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Вертикальное вращение (вверх-вниз) - только для камеры
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minVerticalAngle, maxVerticalAngle);

        // Применяем вертикальное вращение к камере
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Горизонтальное вращение (влево-вправо) - для родительского объекта (игрока)
        if (playerController != null)
        {
            playerController.transform.Rotate(Vector3.up * mouseX);
        }
        else
        {
            // Если CharacterController не найден, вращаем родительский объект
            transform.parent.Rotate(Vector3.up * mouseX);
        }
    }

    // Опционально: разблокировать курсор по нажатию Escape
    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Cursor.lockState = CursorLockMode.None;
        }

        // Заблокировать курсор обратно по клику
        if (Input.GetMouseButtonDown(0) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}