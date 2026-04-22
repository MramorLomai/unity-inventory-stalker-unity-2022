using UnityEngine;

public class WeaponItem : MonoBehaviour
{
    public GameObject viewModelPrefab;

    private GameObject currentViewModel;

    void Start()
    {
        this.enabled = false;
    }

    public void ObjectActive()
    {
        Debug.Log("=== WEAPON ITEM OBJECTACTIVE CALLED ===");
        Debug.Log("Weapon: " + gameObject.name);
        Debug.Log("ViewModelPrefab: " + (viewModelPrefab != null ? viewModelPrefab.name : "NULL"));

        if (currentViewModel != null)
        {
            Debug.Log("Destroying existing viewmodel");
            Destroy(currentViewModel);
        }

        if (viewModelPrefab == null)
        {
            Debug.LogError("NO VIEWMODEL PREFAB ASSIGNED!");
            return;
        }

        // Находим WeaponHolder среди дочерних объектов камеры
        Transform weaponHolder = FindWeaponHolder();

        if (weaponHolder == null)
        {
            Debug.LogError("WEAPON HOLDER NOT FOUND! Creating one...");
            weaponHolder = CreateWeaponHolder();
        }

        currentViewModel = Instantiate(viewModelPrefab);
        currentViewModel.transform.parent = weaponHolder;
        currentViewModel.transform.localPosition = Vector3.zero;
        currentViewModel.transform.localEulerAngles = new Vector3(180, 0, 180);

        Debug.Log("Viewmodel created: " + currentViewModel.name);
        Debug.Log("Parented to: " + weaponHolder.name);
    }

    public void ObjectDeactive()
    {
        Debug.Log("ObjectDeactive called on: " + gameObject.name);

        if (currentViewModel != null)
        {
            Destroy(currentViewModel);
            currentViewModel = null;
        }

        // Уничтожаем тестовый куб
        GameObject testObj = GameObject.Find("TEST_WEAPON_" + gameObject.name);
        if (testObj != null)
        {
            Destroy(testObj);
        }
    }

    private Transform FindWeaponHolder()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("MAIN CAMERA NOT FOUND!");
            return null;
        }

        // Ищем WeaponHolder среди дочерних объектов камеры
        Transform weaponHolder = mainCamera.transform.Find("WeaponHolder");
        if (weaponHolder == null)
        {
            // Альтернативный поиск по имени
            foreach (Transform child in mainCamera.transform)
            {
                if (child.name == "WeaponHolder")
                {
                    weaponHolder = child;
                    break;
                }
            }
        }

        return weaponHolder;
    }

    private Transform CreateWeaponHolder()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Cannot create WeaponHolder - main camera not found!");
            return null;
        }

        // Создаем новый GameObject для WeaponHolder
        GameObject weaponHolder = new GameObject("WeaponHolder");
        weaponHolder.transform.parent = mainCamera.transform;
        weaponHolder.transform.localPosition = Vector3.zero;
        weaponHolder.transform.localRotation = Quaternion.identity;

        Debug.Log("Created new WeaponHolder as child of camera");
        return weaponHolder.transform;
    }
}