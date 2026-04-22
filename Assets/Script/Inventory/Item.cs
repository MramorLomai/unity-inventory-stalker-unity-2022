using UnityEngine;

public class Item : MonoBehaviour
{
    public Inventory.Element item;  // Используем Inventory.Element

    void Start()
    {
        gameObject.tag = "ObjectItem";
    }
}