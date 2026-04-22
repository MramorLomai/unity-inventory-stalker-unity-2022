using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class InventoryBox : MonoBehaviour
{
    // Используем общие типы из Inventory
    public enum IT { Passive, ActiveToClick, ActiveOne, ActiveTwo, ActiveThree, ActiveFour }
    public enum ISI { x64x64, x128x64, x128x128, x256x128 }

    [System.Serializable]
    public class MetaData  // Moved outside Options and renamed to PascalCase
    {
        public int data_0;
        public int data_1;
        public int data_2;
    }

    [System.Serializable]
    public class Element
    {
        [System.Serializable]
        public class Options
        {
            public string name;
            public string description;
            public float mass;
            public float price;
            public float status;
            public Texture2D big_pre = null;
            public MetaData mData;  // Updated reference
        }

        public string name = "NULLITEM";
        public Texture2D sprite = null;
        public IT type = IT.Passive;
        public ISI size = ISI.x64x64;
        public int count = 1;
        public Options option;
        public GameObject _id;

        public Element()
        {
            name = "NULLITEM";
            option = new Options();
            option.mData = new MetaData();  // Updated instantiation
        }

        public Vector2 ISGet()
        {
            Vector2 output;
            switch (size)
            {
                case ISI.x64x64:
                    output = new Vector2(64, 64);
                    break;
                case ISI.x128x64:
                    output = new Vector2(128, 64);
                    break;
                case ISI.x128x128:
                    output = new Vector2(128, 128);
                    break;
                case ISI.x256x128:
                    output = new Vector2(256, 128);
                    break;
                default:
                    output = new Vector2(65, 64);
                    break;
            }
            return output;
        }
    }

    [System.Serializable]
    public class massElement
    {
        public string name = "NULLITEM";
        public Element[] items;
        public int count = 1;
    }

    private GameObject[] G;
    public massElement[] items;
    public bool paypal = true;
    public float npsMoney = 5000;

    public bool toArray(Element Item)
    {
        if (items == null) items = new massElement[0];

        bool res = false;
        int id = -1;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i].name == Item.name)
            {
                if (Item.type == IT.Passive)
                    items[i].count += Item.count;
                else
                    items[i].count += 1;
                id = i;
                break;
            }
        }

        if (id >= 0)
        {
            if (Item.type != IT.Passive)
            {
                List<Element> Contents = new List<Element>(items[id].items);
                Contents.Add(Item);
                items[id].items = Contents.ToArray();
            }
            if (Item.type == IT.Passive)
                items[id].items[0].count += Item.count;
            res = true;
        }
        else
        {
            massElement el = new massElement();
            el.name = Item.name;
            if (Item.type == IT.Passive)
                el.count = Item.count;
            else
                el.count = 1;
            el.items = new Element[1];
            el.items[0] = Item;

            List<massElement> cp = new List<massElement>(items);
            cp.Add(el);
            items = cp.ToArray();
        }
        return res;
    }

    public void drArray(int index)
    {
        if (items == null || index < 0 || index >= items.Length) return;

        if (items[index].items.Length > 1)
        {
            List<Element> Contents = new List<Element>(items[index].items);
            Contents.RemoveAt(0);
            items[index].items = Contents.ToArray();
            items[index].count -= 1;
        }
        else
        {
            List<massElement> cp = new List<massElement>(items);
            cp.RemoveAt(index);
            items = cp.ToArray();
        }
    }

    public void insertToInventory(GameObject insertScene, IT type, bool res, System.Action callback)
    {
        if (!res || type != IT.Passive)
        {
            GameObject g = new GameObject(insertScene.name);
            g.transform.parent = transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            insertScene.transform.parent = g.transform;
            insertScene.transform.localPosition = Vector3.zero;
            insertScene.transform.localRotation = Quaternion.identity;
            insertScene.SetActive(false);
        }
        else
        {
            Destroy(insertScene);
        }
        callback();
    }

    public void CalcElement()
    {
        if (items == null || items.Length < 2) return;

        massElement x;
        for (int i = 0; i <= items.Length - 2; i++)
        {
            for (int j = 0; j <= items.Length - 2; j++)
            {
                if (items[j].items[0].ISGet().x + items[j].items[0].ISGet().y < items[j + 1].items[0].ISGet().x + items[j + 1].items[0].ISGet().y)
                {
                    x = items[j + 1];
                    items[j + 1] = items[j];
                    items[j] = x;
                }
            }
        }
    }

    void Start()
    {
        gameObject.tag = "ShopBox";
        int count = transform.childCount;
        G = new GameObject[count];

        for (int i = 0; i < count; i++)
        {
            G[i] = transform.GetChild(i).gameObject;
        }

        foreach (GameObject itemObj in G)
        {
            Item itemComponent = itemObj.GetComponent<Item>();
            if (itemComponent != null && itemComponent.item != null)
            {
                // Конвертируем Inventory.Element в InventoryBox.Element
                Element boxElement = ConvertFromInventoryElement(itemComponent.item);
                bool res = toArray(boxElement);
                boxElement._id = itemObj;
                insertToInventory(itemObj, boxElement.type, res, delegate () { });
            }
            else
            {
                Debug.LogWarning("Item component or item property is missing on: " + itemObj.name);
            }
        }

        CalcElement();
    }

    // Метод для конвертации из Inventory.Element в InventoryBox.Element
    private Element ConvertFromInventoryElement(Inventory.Element invElement)
    {
        Element boxElement = new Element();
        boxElement.name = invElement.name;
        boxElement.sprite = invElement.sprite;
        boxElement.count = invElement.count;
        boxElement.type = (IT)invElement.type;
        boxElement.size = (ISI)invElement.size;
        boxElement.option = new Element.Options();

        if (invElement.option != null)
        {
            boxElement.option.name = invElement.option.name;
            boxElement.option.description = invElement.option.description;
            boxElement.option.mass = invElement.option.mass;
            boxElement.option.price = invElement.option.price;
            boxElement.option.status = invElement.option.status;
            boxElement.option.big_pre = invElement.option.big_pre;
            boxElement.option.mData = new MetaData();  // Updated instantiation
            if (invElement.option.mData != null)
            {
                boxElement.option.mData.data_0 = invElement.option.mData.data_0;
                boxElement.option.mData.data_1 = invElement.option.mData.data_1;
                boxElement.option.mData.data_2 = invElement.option.mData.data_2;
            }
        }

        return boxElement;
    }

    void Update()
    {
    }
}
