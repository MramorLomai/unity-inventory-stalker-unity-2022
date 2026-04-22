using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    public enum ISI { x64x64, x128x64, x128x128, x256x128 }
    public enum IT { Passive, ActiveToClick, ActiveOne, ActiveTwo, ActiveThree, ActiveFour }

    [System.Serializable]
    public class InventorySound
    {
        public AudioClip invOpen;
        public AudioClip invClose;
        public AudioClip invInsert;
        public AudioClip invDrop;
    }

    [System.Serializable]
    public class gfgui
    {
        public Texture2D background_inv;
        public Texture2D background_box;
        public Texture2D background_pay;
        public Texture2D activeBackDrop;
        public Texture2D StandartArmor;
        public Texture2D center;
        public Texture2D grid;
        public Texture2D slider;
        public GUIStyle Gui;
        public GUIStyle GuiText;
        public GUIStyle GuiBtn;
    }

    private MouseLook mouseLook;

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

    [System.Serializable]
    public class mathfWindow
    {
        public float SCRWidth;
        public float SCRHeight;
        public float slise;
        public Vector2 windowSize;
        public Vector2 windowSizeP;
        public Vector2 SizeBorder;
        public float ScaleIcons;
        public float ScaleIconsP;
        public Rect[] Actives;
        public float topout;
        public float topoutP;
        public Rect armor;

        [System.Serializable]
        public class infoRect
        {
            public Rect actorMoney;
            public Rect name;
            public Rect description;
            public Rect price;
        }

        public infoRect inf;
        public float border;
        public float scrollLengthNext;
        public Rect activeBackDrop;
        public Rect background;
        public Rect leftBox;
        public Rect RightBox;

        public float round_to(float d)
        {
            d = Mathf.Floor(d);
            int i = (int)d;
            if (i % 10 != 0) i = (i / 10) * 10 + 10;
            d = i;
            return d;
        }

        public void init()
        {
            inf = new infoRect();
            SCRWidth = Screen.width;
            SCRHeight = Screen.height;
            slise = 0.0f;
            slise = (Screen.width - SCRWidth) / 2;
            windowSize.x = (SCRWidth - 100) * 0.325f;
            windowSize.y = (SCRHeight - 200) * 0.92f;
            windowSizeP.x = (SCRWidth - 100) * 0.47f;
            windowSizeP.y = (SCRHeight - 200) * 0.915f;
            ScaleIcons = windowSize.x * 0.00156f;
            ScaleIconsP = windowSizeP.x * 0.00156f;
            border = ((SCRWidth - 300) * 0.1f) * 0.0085f;
            armor = new Rect((SCRWidth - 50) * 0.87f, 35 + (SCRHeight) * 0.30f, ((SCRWidth - 50) * 0.10f), SCRHeight * 0.30f);
            inf.name = new Rect(SCRWidth * 0.396f, 35 + (SCRHeight) * 0.445f, SCRWidth / 6.21f, 30);
            inf.price = new Rect(SCRWidth * 0.57f, 35 + (SCRHeight) * 0.445f, SCRWidth / 10.6f, 30);
            inf.actorMoney = new Rect((SCRWidth - 50) * 0.745f, (SCRHeight - 35) * 0.6f + 35, (SCRWidth - 50) / 9.7f, SCRHeight / 18);
            inf.description = new Rect(SCRWidth * 0.396f, 35 + (SCRHeight) * 0.525f, windowSize.x - SizeBorder.x * 2, 200);
            activeBackDrop = new Rect(100, 0, SCRWidth - 200, 137);
            background = new Rect(50, 140, SCRWidth - 100, SCRHeight - 200);
            topout = ((SCRHeight - 137) / 16.74f) + 137;
            topoutP = ((SCRHeight - 137) / 17f) + 137;
            SizeBorder.x = (SCRWidth) * 0.0145f;
            leftBox = new Rect((SCRWidth * 0.018f) + 50, topoutP, windowSizeP.x, windowSizeP.y);
            RightBox = new Rect((SCRWidth * 0.509f), topoutP, windowSizeP.x, windowSizeP.y);
            scrollLengthNext = 64 * ScaleIcons;
        }
    }

    public Transform ActorCamera;
    public gfgui TxGUI;
    public InventorySound soundFx;
    public massElement[] items;

    private mathfWindow mtp;
    private Vector2 windowSize = new Vector2(512, 512);
    private Vector2 SizeBorder = new Vector2(50, 59);
    private Element[] Actives;
    private Element[] passActive;
    private Element[] npsTemp;
    private Element[] actTemp;
    private float ScaleIcons = 1.4f;
    private Vector2 scrollPosition = Vector2.zero;
    private Vector2 scrollPositionx = Vector2.zero;
    private Vector2 scrollPositiona = Vector2.zero;
    private Vector2 scrollPositionb = Vector2.zero;
    private bool showGUI = false;
    private float maxX = 0;
    private float maxY = 0;
    private float dclick = 0.0f;
    private int selected = -1;
    private bool itemIcon = false;
    private Texture2D Icon;
    private float InventoryMass;
    private float scrollLength = 0;
    private float scrollLengthx = 0;
    private float scrollLengtha = 0;
    private float scrollLengthb = 0;
    private bool showShop = false;
    private bool paypal = true;
    private bool goodShop = false;
    private GameObject ShopGm;
    public float TOP = 0.00f;
    public Rect roots;
    public Texture2D IMX;

    void Start()
    {
        Actives = new Element[3];
        for (int i = 0; i < Actives.Length; i++)
        {
            Actives[i] = new Element();
        }
        passActive = new Element[0];
        actTemp = new Element[0];
        npsTemp = new Element[0];
        mtp = new mathfWindow();
        mtp.init();

        mouseLook = ActorCamera.GetComponent<MouseLook>();
        if (mouseLook == null)
        {
            mouseLook = GetComponentInParent<MouseLook>();
        }

        UpdateCursorState();
    }

    bool searchElement(string name, int count)
    {
        bool bing = false;
        if (items != null)
        {
            foreach (massElement itm in items)
            {
                if (itm.name == name)
                {
                    itm.count += count;
                    bing = true;
                    break;
                }
            }
        }
        return bing;
    }

    void startScript(GameObject g)
    {
        MonoBehaviour[] cp = g.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour c in cp)
            if (c.enabled == false)
                c.SendMessage("Start");
    }

    MonoBehaviour getScript(GameObject g)
    {
        MonoBehaviour res = null;
        MonoBehaviour[] cp = g.GetComponents<MonoBehaviour>();
        Debug.Log("Searching for scripts on: " + g.name);
        foreach (MonoBehaviour c in cp)
        {
            Debug.Log("Found script: " + c.GetType().Name + ", enabled: " + c.enabled);
            if (c.GetType().Name == "WeaponItem")
            {
                res = c;
                break;
            }
        }
        return res;
    }

    MonoBehaviour getScripto(GameObject g)
    {
        MonoBehaviour cp = g.GetComponent<MonoBehaviour>();
        return cp;
    }

    public int APIgetItemCount(string name)
    {
        int bing = 0;
        if (items != null)
        {
            foreach (massElement itm in items)
            {
                if (itm.name == name)
                {
                    bing = itm.count;
                    break;
                }
            }
        }
        return bing;
    }

    public void APISetActivesData(int id, int data_id, int data)
    {
        if (Actives == null || id >= Actives.Length || Actives[id] == null) return;

        if (Actives[id].option == null) Actives[id].option = new Element.Options();
        if (Actives[id].option.mData == null) Actives[id].option.mData = new MetaData();  // Updated

        switch (data_id)
        {
            case 0:
                Actives[id].option.mData.data_0 = data;
                break;
            case 1:
                Actives[id].option.mData.data_1 = data;
                break;
            case 2:
                Actives[id].option.mData.data_2 = data;
                break;
        }
    }

    public int APIGetActivesData(int id, int data_id)
    {
        int output = -1;
        if (Actives == null || id >= Actives.Length || Actives[id] == null) return -1;
        if (Actives[id].option == null || Actives[id].option.mData == null) return -1;  // Updated

        switch (data_id)
        {
            case 0:
                output = Actives[id].option.mData.data_0;
                break;
            case 1:
                output = Actives[id].option.mData.data_1;
                break;
            case 2:
                output = Actives[id].option.mData.data_2;
                break;
        }
        return output;
    }

    public void APIeditItemCount(string name, int count)
    {
        if (items != null)
        {
            foreach (massElement itm in items)
            {
                if (itm.name == name)
                {
                    itm.count = count;
                    if (itm.items.Length > 0)
                        itm.items[0].count = count;
                    break;
                }
            }
        }
    }

    bool toArray(Element Item)
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

    void drArray(int index)
    {
        Debug.Log("drArray called with index: " + index + ", items length: " + items.Length);

        if (items == null || index < 0 || index >= items.Length)
        {
            Debug.LogError("Invalid index in drArray: " + index);
            return;
        }

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

        Debug.Log("drArray completed, new items length: " + items.Length);
    }

    void toArrayPass(Element Item)
    {
        List<Element> Contents = new List<Element>(passActive);
        Contents.Add(Item);
        passActive = Contents.ToArray();
    }

    void drArrayPass(int index)
    {
        if (index < 0 || index >= passActive.Length) return;

        List<Element> Contents = new List<Element>(passActive);
        Contents.RemoveAt(index);
        passActive = Contents.ToArray();
    }

    void CalcElement()
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

        float widthm = showShop || paypal ? 896 : 640;

        float xt = 0;
        float maxY = 0;
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null || items[i].items.Length == 0) continue;

            float ix = items[i].items[0].ISGet().x;
            float iy = items[i].items[0].ISGet().y;
            if (iy > maxY)
            {
                maxY = iy;
            }
            if (xt + ix < widthm)
            {
                xt += ix;
            }
            else
            {
                for (int j = i; j < items.Length; j++)
                {
                    if (items[j] == null || items[j].items.Length == 0) continue;

                    if (xt + items[j].items[0].ISGet().x == widthm)
                    {
                        x = items[i];
                        items[i] = items[j];
                        items[j] = x;
                        xt = 0;
                        maxY = 0;
                        break;
                    }
                }
            }
        }
    }

    void insertToScene(GameObject insertScene, Element it)
    {
        if (insertScene.transform.parent != null)
        {
            GameObject p = insertScene.transform.parent.gameObject;
            insertScene.transform.parent = null;
            Destroy(p);
        }
        insertScene.transform.position = transform.position + ActorCamera.TransformDirection(new Vector3(0, 0, 0.5f));
        insertScene.transform.rotation = transform.rotation;
        insertScene.SetActive(true);

        Item itemComponent = insertScene.GetComponent<Item>();
        if (itemComponent != null)
        {
            itemComponent.item = it;
        }
    }

    void insertToInventory(GameObject insertScene, IT type, bool res)
    {
        if (!res || type != IT.Passive)
        {
            insertScene.SetActive(false);
            GameObject g = new GameObject(insertScene.name);
            g.transform.parent = transform;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            if (type != IT.Passive)
            {
                MonoBehaviour script = getScript(insertScene);
                if (script != null)
                {
                    CopyComponent(script, g).enabled = false;
                }
            }
            insertScene.transform.parent = g.transform;
            insertScene.transform.localPosition = Vector3.zero;
            insertScene.transform.localRotation = Quaternion.identity;
        }
        else
        {
            Destroy(insertScene);
        }
    }

    MonoBehaviour CopyComponent(MonoBehaviour original, GameObject destination)
    {
        System.Type type = original.GetType();
        MonoBehaviour copy = destination.AddComponent(type) as MonoBehaviour;
        System.Reflection.FieldInfo[] fields = type.GetFields();
        foreach (System.Reflection.FieldInfo field in fields)
        {
            field.SetValue(copy, field.GetValue(original));
        }
        return copy;
    }

    public void AddItem(Element Item, GameObject g)
    {
        bool res = toArray(Item);
        insertToInventory(g, Item.type, res);
        CalcElement();
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
    }

    void removeItem(int index)
    {
        if (index < 0 || index >= items.Length) return;
        if (items[index].items.Length == 0) return;

        insertToScene(items[index].items[0]._id, items[index].items[0]);
        drArray(index);
        CalcElement();
        GetComponent<AudioSource>().clip = soundFx.invDrop;
        GetComponent<AudioSource>().Play();
    }

    void dropInActive(int type, System.Action cb)
    {
        if (Actives[type].name != "NULLITEM")
        {
            toArray(Actives[type]);
            MonoBehaviour script = getScripto(Actives[type]._id.transform.parent.gameObject);
            if (script != null)
            {
                script.SendMessage("ObjectDeactive", SendMessageOptions.DontRequireReceiver);
                script.enabled = false;
            }
            Actives[type] = new Element();
        }
        CalcElement();
        cb();
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
    }

    void insertToActive(int type, int index)
    {
        // Добавим проверку на валидность индекса
        if (index < 0 || index >= items.Length)
        {
            Debug.LogError("Invalid index in insertToActive: " + index + ", items length: " + items.Length);
            return;
        }

        if (items[index].items.Length == 0)
        {
            Debug.LogError("No items at index: " + index);
            return;
        }

        dropInActive(type, delegate ()
        {
            Debug.Log("Starting activation for item: " + items[index].items[0].name);

            // Проверяем что индекс все еще валиден после drArray
            if (index >= items.Length || items[index].items.Length == 0)
            {
                Debug.LogError("Index became invalid after drArray: " + index);
                return;
            }

            Actives[type] = items[index].items[0];
            drArray(index);
            selected = -1;

            MonoBehaviour script = getScript(Actives[type]._id.transform.parent.gameObject);
            if (script != null)
            {
                Debug.Log("Calling ObjectActive on: " + script.GetType().Name);
                script.SendMessage("ObjectActive", SendMessageOptions.DontRequireReceiver);

                WeaponController weaponController = script as WeaponController;
                if (weaponController != null)
                {
                    weaponController.LoadFromInventory(type);
                }

                script.enabled = true;

            }
            else
            {
                Debug.LogError("No script found for activation!");
            }
        });
        CalcElement();
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
    }

    void insertToPassActive(int index)
    {
        if (passActive.Length < 8)
        {
            MonoBehaviour ObjectActive = getScript(items[index].items[0]._id.transform.parent.gameObject);
            toArrayPass(items[index].items[0]);
            drArray(index);
            if (ObjectActive != null)
            {
                ObjectActive.enabled = true;
                ObjectActive.SendMessage("ObjectActive", SendMessageOptions.DontRequireReceiver);
            }
            selected = -1;
        }
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
    }

    void dropToPassActive(int index)
    {
        if (index < 0 || index >= passActive.Length) return;

        MonoBehaviour ObjectActive = getScripto(passActive[index]._id.transform.parent.gameObject);
        if (ObjectActive != null)
        {
            ObjectActive.enabled = false;
        }
        toArray(passActive[index]);
        drArrayPass(index);
        CalcElement();
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
    }

    void clickActive(int index)
    {
        startScript(items[index].items[0]._id.transform.parent.gameObject);
        GameObject tmps = items[index].items[0]._id.transform.parent.gameObject;
        Destroy(items[index].items[0]._id);
        Destroy(tmps);
        drArray(index);
        selected = -1;
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
    }

    void clickItem(IT type, int index)
    {
        switch (type)
        {
            case IT.Passive:
                break;
            case IT.ActiveToClick:
                clickActive(index);
                break;
            case IT.ActiveOne:
                insertToActive(0, index);
                break;
            case IT.ActiveTwo:
                insertToActive(1, index);
                break;
            case IT.ActiveThree:
                insertToPassActive(index);
                break;
            case IT.ActiveFour:
                insertToActive(2, index);
                break;
            default:
                break;
        }
    }

    // Вспомогательный метод для конвертации Element в InventoryBox.Element
    private InventoryBox.Element ConvertToInventoryBoxElement(Element elem)
    {
        InventoryBox.Element newElem = new InventoryBox.Element();
        newElem.name = elem.name;
        newElem.sprite = elem.sprite;
        newElem.count = elem.count;
        newElem.type = (InventoryBox.IT)elem.type;
        newElem.size = (InventoryBox.ISI)elem.size;
        newElem.option = new InventoryBox.Element.Options();
        if (elem.option != null)
        {
            newElem.option.name = elem.option.name;
            newElem.option.description = elem.option.description;
            newElem.option.mass = elem.option.mass;
            newElem.option.price = elem.option.price;
            newElem.option.status = elem.option.status;
            newElem.option.big_pre = elem.option.big_pre;
        }
        newElem._id = elem._id;
        return newElem;
    }

    // Вспомогательный метод для обратной конвертации
    private Element ConvertFromInventoryBoxElement(InventoryBox.Element elem)
    {
        Element newElem = new Element();
        newElem.name = elem.name;
        newElem.sprite = elem.sprite;
        newElem.count = elem.count;
        newElem.type = (IT)elem.type;
        newElem.size = (ISI)elem.size;
        newElem.option = new Element.Options();
        if (elem.option != null)
        {
            newElem.option.name = elem.option.name;
            newElem.option.description = elem.option.description;
            newElem.option.mass = elem.option.mass;
            newElem.option.price = elem.option.price;
            newElem.option.status = elem.option.status;
            newElem.option.big_pre = elem.option.big_pre;
        }
        newElem._id = elem._id;
        return newElem;
    }

    void boxToInt(Element item, System.Action cb)
    {
        GameObject tmp = item._id.transform.parent.gameObject;
        insertToInventory(item._id, item.type, toArray(item));
        Destroy(tmp);
        CalcElement();
        cb();
        selected = -1;
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
    }

    void intToBox(Element item, System.Action c)
    {
        GameObject tmp = item._id.transform.parent.gameObject;
        InventoryBox.Element boxElement = ConvertToInventoryBoxElement(item);
        ShopGm.GetComponent<InventoryBox>().insertToInventory(item._id, (InventoryBox.IT)item.type, ShopGm.GetComponent<InventoryBox>().toArray(boxElement), delegate ()
        {
            Destroy(tmp);
            ShopGm.GetComponent<InventoryBox>().CalcElement();
        });
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
        selected = -1;
        c();
    }

    void invToAct(Element item, System.Action c)
    {
        List<Element> Contents = new List<Element>(actTemp);
        Contents.Add(item);
        actTemp = Contents.ToArray();
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
        selected = -1;
        c();
    }

    void npsToAct(Element item, System.Action c)
    {
        List<Element> Contents = new List<Element>(npsTemp);
        Contents.Add(item);
        npsTemp = Contents.ToArray();
        GetComponent<AudioSource>().clip = soundFx.invInsert;
        GetComponent<AudioSource>().Play();
        selected = -1;
        c();
    }

    void goShop()
    {
        float actorMoney = GameControl.actorMoney;
        float npsMoney = ShopGm.GetComponent<InventoryBox>().npsMoney;
        float npsShop = 0.0f;
        float actShop = 0.0f;
        if (actTemp.Length == 0 && npsTemp.Length == 0)
        {
            Debug.Log("Buy not fond");
        }
        else
        {
            foreach (Element itm in actTemp)
            {
                if (itm.option != null)
                    npsShop += itm.option.price;
            }
            foreach (Element itm in npsTemp)
            {
                if (itm.option != null)
                    actShop += itm.option.price;
            }
            if (npsMoney >= npsShop && actTemp.Length > 0)
            {
                foreach (Element itm in actTemp)
                {
                    intToBox(itm, delegate ()
                    {
                        CalcElement();
                    });
                }
                GameControl.actorMoney += npsShop;
                ShopGm.GetComponent<InventoryBox>().npsMoney -= npsShop;
                actTemp = new Element[0];
            }
            else
            {
                if (actTemp.Length > 0)
                    Debug.Log("NPC NO MONEY");
            }

            if (actorMoney >= actShop && npsTemp.Length > 0)
            {
                foreach (Element itm in npsTemp)
                {
                    boxToInt(itm, delegate () { });
                }
                GameControl.actorMoney -= actShop;
                ShopGm.GetComponent<InventoryBox>().npsMoney += actShop;
                npsTemp = new Element[0];
            }
            else
            {
                if (npsTemp.Length > 0)
                    Debug.Log("YOU DO NOT HAVE MONEY");
            }
        }
    }

    void noShop()
    {
        foreach (Element itm in npsTemp)
        {
            InventoryBox.Element boxElement = ConvertToInventoryBoxElement(itm);
            ShopGm.GetComponent<InventoryBox>().toArray(boxElement);
        }
        npsTemp = new Element[0];
        foreach (Element itm in actTemp)
        {
            toArray(itm);
        }
        actTemp = new Element[0];
        goodShop = true;
    }

    void DrawTiled(Rect rect, Texture2D tex, float size, int count)
    {
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        float dx = (tex.width * (size));
        float cell = (64 * (size + 0.00385f));
        float dy = tex.height * (size);
        GUI.BeginGroup(new Rect(rect.x, rect.y, cell * count, height));
        for (int y = 0; y < height; y += (int)dy)
        {
            GUI.DrawTexture(new Rect(0, y, width, dy), tex);
        }
        GUI.EndGroup();
    }

    void OnGUI()
    {
        UpdateCursorState();

        if (!showShop && !goodShop)
            noShop();
        if (!showGUI && !showShop)
        {
            GameControl.ActivateGUI = false;
            // Обновляем состояние курсора
            UpdateCursorState();
        }

        GUI.DrawTexture(new Rect(Screen.width / 2 - 16, Screen.height / 2 - 16, 32, 32), TxGUI.center);

        if (showGUI)
        {
            mtp.init();
            int index = 0;
            maxX = 0;
            maxY = 0;
            int idx = 0;
            float ix = 0.0f;
            float iy = 0.0f;
            float currentX = 0;
            float currentY = 0;
            GUI.BeginGroup(new Rect(mtp.slise, 0, mtp.SCRWidth, Screen.height));

            showShop = false;
            GameControl.ActivateGUI = true;
            GUI.DrawTexture(mtp.activeBackDrop, TxGUI.activeBackDrop, ScaleMode.StretchToFill);
            GUI.DrawTexture(mtp.background, TxGUI.background_inv, ScaleMode.StretchToFill);

            if (Actives[0].name != "NULLITEM")
            {
                if (GUI.Button(new Rect(mtp.SCRWidth / 2 - TxGUI.activeBackDrop.width / 2 + 50, 25, Actives[0].ISGet().x, Actives[0].ISGet().y), Actives[0].sprite, TxGUI.Gui))
                {
                    if (Time.time > dclick + 0.3f)
                    {
                        dclick = Time.time;
                    }
                    else
                    {
                        dropInActive(0, delegate () { });
                    }
                }
            }

            if (Actives[1].name != "NULLITEM")
            {
                if (GUI.Button(new Rect((mtp.SCRWidth - 300) * 0.38f, 10, Actives[1].ISGet().x, Actives[1].ISGet().y), Actives[1].sprite, TxGUI.Gui))
                {
                    if (Time.time > dclick + 0.3f)
                    {
                        dclick = Time.time;
                    }
                    else
                    {
                        dropInActive(1, delegate () { });
                    }
                }
            }

            if (Actives[2].name != "NULLITEM")
            {
                if (GUI.Button(mtp.armor, Actives[2].option.big_pre, TxGUI.Gui))
                {
                    if (Time.time > dclick + 0.3f)
                    {
                        dclick = Time.time;
                    }
                    else
                    {
                        dropInActive(2, delegate () { });
                    }
                }
            }
            else
            {
                GUI.DrawTexture(mtp.armor, TxGUI.StandartArmor, ScaleMode.StretchToFill);
            }

            if (passActive.Length > 0)
            {
                float cX = 0;
                float cY = 0;
                foreach (Element item in passActive)
                {
                    if (GUI.Button(new Rect(cX + (mtp.SCRWidth * 0.565f), 40, mtp.scrollLengthNext, mtp.scrollLengthNext), item.sprite, TxGUI.Gui))
                    {
                        if (Time.time > dclick + 0.3f) dclick = Time.time; else dropToPassActive(idx);
                    }
                    cX += mtp.scrollLengthNext; idx++;
                }
            }

            scrollPosition = GUI.BeginScrollView(new Rect(50 + mtp.SizeBorder.x, mtp.topout, mtp.windowSize.x, mtp.windowSize.y), scrollPosition, new Rect(0, 0, 0, scrollLength));
            if (IMX != null)
                GUI.DrawTexture(new Rect(0, 0, 2000, 2000), IMX);
            scrollLength = scrollLength < mtp.windowSize.y ? mtp.windowSize.y : scrollLength;
            DrawTiled(new Rect(0, 0, mtp.windowSize.x, scrollLength), TxGUI.grid, mtp.ScaleIcons, 10);
            scrollLength = 0;

            if (items != null)
            {
                foreach (massElement item in items)
                {
                    if (item.items.Length == 0) continue;

                    ix = (item.items[0].ISGet().x + 1) * mtp.ScaleIcons;
                    iy = item.items[0].ISGet().y * mtp.ScaleIcons;
                    maxX = maxX == mtp.windowSizeP.x ? 0 : maxX;
                    if (mtp.round_to(currentX + ix) > mtp.round_to(mtp.windowSize.x))
                    {
                        currentX = maxX;
                        currentY += iy;
                        currentX = (currentY + iy >= currentY + maxY) ? 0 : currentX;
                        maxX = 0; maxY = 0;
                    }

                    if (GUI.Button(new Rect(currentX, currentY, ix, iy), item.items[0].sprite, TxGUI.Gui))
                    {
                        selected = index;
                        if (Time.time > dclick + 0.3f)
                        {
                            dclick = Time.time;
                        }
                        else
                        {
                            clickItem(item.items[0].type, index);
                            dclick = Time.time;
                        }
                    }
                    if (item.count > 1)
                        GUI.Label(new Rect(currentX + 5, currentY + 5, item.items[0].ISGet().x, item.items[0].ISGet().y), "x" + item.count.ToString());

                    currentX += ix;
                    if (maxY <= iy)
                    {
                        maxY = iy;
                        maxX += ix;
                    }
                    index++;
                }
            }
            scrollLength = mtp.scrollLengthNext + currentY + maxY;
            GUI.EndScrollView();

            GUI.Label(mtp.inf.actorMoney, GameControl.actorMoney.ToString(), TxGUI.GuiText);

            if (selected >= 0 && items != null && selected < items.Length && items[selected].items.Length > 0)
            {
                Element itm = items[selected].items[0];
                if (itm.option != null)
                {
                    GUI.Label(mtp.inf.name, itm.option.name, TxGUI.GuiText);
                    GUI.Label(mtp.inf.description, itm.option.description);
                    GUI.Label(mtp.inf.price, itm.option.price.ToString(), TxGUI.GuiText);
                    GUI.DrawTexture(new Rect(mtp.SCRWidth * 0.57f, 37 + (Screen.height * 0.501f), Mathf.FloorToInt((mtp.SCRWidth / 10.3f) / 100 * itm.option.status), 4), TxGUI.slider);
                }
                GUI.DrawTexture(new Rect((mtp.SCRWidth * 0.53f) - (itm.ISGet().x / 2), Screen.height * 0.26f, itm.ISGet().x, itm.ISGet().y), itm.sprite, ScaleMode.StretchToFill);
            }
            GUI.EndGroup();
        }

        if (showShop)
        {
            mtp.init();

            int index = 0; goodShop = false; float currentX = 0; float currentY = 0; float maxX = 0; float maxY = 0;
            GUI.BeginGroup(new Rect(mtp.slise, 0, mtp.SCRWidth, Screen.height));
            GUI.DrawTexture(mtp.background, TxGUI.background_box, ScaleMode.StretchToFill);

            scrollPosition = GUI.BeginScrollView(mtp.leftBox, scrollPosition, new Rect(0, 0, 0, scrollLength));
            scrollLength = scrollLength < mtp.windowSizeP.y ? mtp.windowSizeP.y : scrollLength;
            DrawTiled(new Rect(0, 0, mtp.windowSizeP.x, scrollLength), TxGUI.grid, mtp.ScaleIcons, 15);
            scrollLength = 0;

            if (items != null)
            {
                foreach (massElement item in items)
                {
                    if (item.items.Length == 0) continue;

                    float ix = (item.items[0].ISGet().x + 1) * mtp.ScaleIcons;
                    float iy = item.items[0].ISGet().y * mtp.ScaleIcons;
                    maxX = maxX == mtp.windowSizeP.x ? 0 : maxX;
                    if (mtp.round_to(currentX + ix) > mtp.round_to(mtp.windowSizeP.x))
                    {
                        currentX = maxX;
                        currentY += iy;
                        currentX = (currentY + iy >= currentY + maxY) ? 0 : currentX;
                        maxX = 0; maxY = 0;
                    }
                    if (GUI.Button(new Rect(currentX, currentY, ix, iy), item.items[0].sprite, TxGUI.Gui))
                    {
                        selected = index;
                        if (Time.time > dclick + 0.3f)
                        {
                            dclick = Time.time;
                        }
                        else
                        {
                            if (paypal)
                            {
                                invToAct(item.items[0], delegate ()
                                {
                                    drArray(index);
                                });
                            }
                            else
                            {
                                intToBox(item.items[0], delegate ()
                                {
                                    drArray(index);
                                    CalcElement();
                                });
                            }
                            dclick = Time.time;
                        }
                    }

                    if (item.count > 1)
                        GUI.Label(new Rect(currentX + 5, currentY + 5, item.items[0].ISGet().x, item.items[0].ISGet().y), "x" + item.count.ToString());
                    currentX += ix;
                    if (maxY <= iy)
                    {
                        maxY = iy;
                        maxX += ix;
                    }
                    index++;
                }
            }
            scrollLength = 64 * mtp.ScaleIcons + currentY + maxY;
            GUI.EndScrollView();

            index = 0; currentX = 0; currentY = 0; maxX = 0; maxY = 0;
            scrollPositionx = GUI.BeginScrollView(mtp.RightBox, scrollPositionx, new Rect(0, 0, 0, scrollLengthx));
            if (scrollLength < mtp.windowSize.y) scrollLengthx = mtp.windowSizeP.y;
            DrawTiled(new Rect(0, 0, mtp.windowSizeP.x, scrollLengthx), TxGUI.grid, mtp.ScaleIcons, 15);
            scrollLength = 0;

            if (ShopGm != null)
            {
                paypal = ShopGm.GetComponent<InventoryBox>().paypal;
                InventoryBox inventoryBox = ShopGm.GetComponent<InventoryBox>();
                if (inventoryBox.items != null)
                {
                    foreach (InventoryBox.massElement item in inventoryBox.items)
                    {
                        if (item.items.Length == 0) continue;

                        float ix = (item.items[0].ISGet().x + 1) * mtp.ScaleIcons;
                        float iy = item.items[0].ISGet().y * mtp.ScaleIcons;
                        if (maxX == mtp.windowSizeP.x) { maxX = 0; }
                        if (mtp.round_to(currentX + ix) > mtp.round_to(mtp.windowSizeP.x))
                        {
                            currentX = maxX;
                            currentY += iy;
                            if (currentY + iy >= currentY + maxY)
                            {
                                currentX = 0;
                            }
                            maxX = 0; maxY = 0;
                        }
                        if (GUI.Button(new Rect(currentX, currentY, ix, iy), item.items[0].sprite, TxGUI.Gui))
                        {
                            selected = index;
                            if (Time.time > dclick + 0.3f)
                            {
                                dclick = Time.time;
                            }
                            else
                            {
                                if (paypal)
                                {
                                    Element convertedItem = ConvertFromInventoryBoxElement(item.items[0]);
                                    npsToAct(convertedItem, delegate ()
                                    {
                                        inventoryBox.drArray(index);
                                        inventoryBox.CalcElement();
                                    });
                                }
                                else
                                {
                                    Element convertedItem = ConvertFromInventoryBoxElement(item.items[0]);
                                    boxToInt(convertedItem, delegate ()
                                    {
                                        inventoryBox.drArray(index);
                                        inventoryBox.CalcElement();
                                    });
                                }
                                dclick = Time.time;
                            }
                        }
                        if (item.count > 1)
                            GUI.Label(new Rect(currentX + 5, currentY + 5, item.items[0].ISGet().x, item.items[0].ISGet().y), "x" + item.count.ToString());
                        currentX += ix;
                        if (maxY <= iy)
                        {
                            maxY = iy;
                            maxX += ix;
                        }
                        index++;
                    }
                }
            }
            scrollLengthx = 64 * mtp.ScaleIcons + currentY + maxY;
            GUI.EndScrollView();

            if (paypal)
            {
                index = 0;
                currentX = 0; currentY = 0; maxX = 0; maxY = 0;
                mtp.windowSize.x = mtp.SCRWidth * 0.3074953125f;
                mtp.ScaleIcons = mtp.windowSize.x * 0.001953f;
                mtp.windowSize.y = (Screen.height - TxGUI.activeBackDrop.height) * 0.365f;
                Rect Rects = new Rect(SizeBorder.x + mtp.windowSize.x / 2, mtp.windowSize.y * 3 + TxGUI.activeBackDrop.height - 120, mtp.windowSize.x * 0.5f, 30);

                GUI.Label(new Rect(Mathf.FloorToInt(mtp.SCRWidth / 19.45f), Mathf.FloorToInt(Screen.height / 4.34f), 100, 30), GameControl.actorMoney.ToString(), TxGUI.GuiText);
                if (ShopGm != null)
                    GUI.Label(new Rect(Mathf.FloorToInt(mtp.SCRWidth / 1.165f), Mathf.FloorToInt(Screen.height / 4.34f), 100, 30), ShopGm.GetComponent<InventoryBox>().npsMoney.ToString(), TxGUI.GuiText);

                scrollPositiona = GUI.BeginScrollView(new Rect(SizeBorder.x, TxGUI.activeBackDrop.height + 40, mtp.windowSize.x, mtp.windowSize.y), scrollPositiona, new Rect(0, 0, 0, scrollLengtha));
                GUI.DrawTextureWithTexCoords(new Rect(0, 0, mtp.windowSize.x, scrollLengtha), TxGUI.grid, new Rect(0, 0, 8.05f, scrollLengtha / (TxGUI.grid.height * mtp.ScaleIcons)));
                scrollLengtha = 0;

                foreach (Element item in actTemp)
                {
                    if (GUI.Button(new Rect(currentX, currentY, item.ISGet().x * mtp.ScaleIcons, item.ISGet().y * mtp.ScaleIcons), item.sprite, TxGUI.Gui))
                    {
                        selected = index;
                        if (Time.time > dclick + 0.3f)
                        {
                            dclick = Time.time;
                        }
                        else
                        {
                            toArray(item);
                            CalcElement();
                            List<Element> cp = new List<Element>(actTemp);
                            cp.RemoveAt(index);
                            actTemp = cp.ToArray();
                            dclick = Time.time;
                        }
                    }
                    if (item.count > 1)
                        GUI.Label(new Rect(currentX + 5, currentY + 5, item.ISGet().x, item.ISGet().y), "x" + item.count.ToString());
                    index++;
                }
                scrollLengtha = 64 * mtp.ScaleIcons + currentY + maxY;
                GUI.EndScrollView();

                index = 0;
                currentX = 0; currentY = 0; maxX = 0; maxY = 0; SizeBorder.x = (mtp.SCRWidth) * 0.352f;
                mtp.windowSize.x = mtp.SCRWidth * 0.3074953125f;
                mtp.ScaleIcons = mtp.windowSize.x * 0.001953f;
                mtp.windowSize.y = (Screen.height - TxGUI.activeBackDrop.height) * 0.3655f;
                scrollPositionb = GUI.BeginScrollView(new Rect(SizeBorder.x, TxGUI.activeBackDrop.height + mtp.windowSize.y + 106, mtp.windowSize.x, mtp.windowSize.y), scrollPositionb, new Rect(0, 0, 0, scrollLengthb));
                GUI.DrawTextureWithTexCoords(new Rect(0, 0, mtp.windowSize.x, scrollLengthb), TxGUI.grid, new Rect(0, 0, 8.05f, scrollLengthb / (TxGUI.grid.height * mtp.ScaleIcons)));
                scrollLengthb = 0;

                foreach (Element item in npsTemp)
                {
                    if (GUI.Button(new Rect(currentX, currentY, item.ISGet().x * mtp.ScaleIcons, item.ISGet().y * mtp.ScaleIcons), item.sprite, TxGUI.Gui))
                    {
                        selected = index;
                        if (Time.time > dclick + 0.3f)
                        {
                            dclick = Time.time;
                        }
                        else
                        {
                            if (ShopGm != null)
                            {
                                InventoryBox.Element boxElement = ConvertToInventoryBoxElement(item);
                                ShopGm.GetComponent<InventoryBox>().toArray(boxElement);
                                ShopGm.GetComponent<InventoryBox>().CalcElement();
                            }
                            List<Element> cps = new List<Element>(npsTemp);
                            cps.RemoveAt(index);
                            npsTemp = cps.ToArray();
                            dclick = Time.time;
                        }
                    }
                    if (item.count > 1)
                        GUI.Label(new Rect(currentX + 5, currentY + 5, item.ISGet().x, item.ISGet().y), "x" + item.count.ToString());
                    index++;
                }

                scrollLengthb = 64 * mtp.ScaleIcons + currentY + maxY;
                GUI.EndScrollView();
                if (GUI.Button(Rects, "Торговать", TxGUI.GuiBtn))
                {
                    goShop();
                }
            }

            GUI.EndGroup();
        }

        if (itemIcon && Icon != null)
        {
            GUI.Label(new Rect(Screen.width / 2 - 10, Screen.height / 2 + Screen.height / 3, 150, 40), "GET");
            GUI.DrawTexture(new Rect(Screen.width / 2 - Icon.width / 2, Screen.height / 2 + Screen.height / 3 + 10, Icon.width, Icon.height), Icon);
        }
    }

    private void UpdateCursorState()
    {
        bool inventoryOpen = showGUI || showShop;

        if (inventoryOpen)
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (mouseLook != null)
            {
                mouseLook.enabled = false;
            }
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (mouseLook != null)
            {
                mouseLook.enabled = true;
            }
        }
    }

    void keyStat()
    {
        if (Input.GetKeyUp("i"))
        {
            CalcElement();
            mtp.init();
            if (showGUI)
            {
                GetComponent<AudioSource>().clip = soundFx.invClose;
                GetComponent<AudioSource>().Play();
            }
            else
            {
                GetComponent<AudioSource>().clip = soundFx.invOpen;
                GetComponent<AudioSource>().Play();
            }
            showGUI = !showGUI;
            GameControl.ActivateGUI = GameControl.ActivateGUI ? false : true;

            UpdateCursorState();
        }

        if (Input.GetKeyUp("g"))
            if (selected >= 0)
            {
                removeItem(selected);
                selected = -1;
            }
    }


    void objectItemAdd()
    {
        if (showGUI || showShop) return;

        RaycastHit hit;
        Vector3 dir = ActorCamera.TransformDirection(new Vector3(0, 0, 1));
        if (Physics.Raycast(ActorCamera.position, dir, out hit, 4))
        {
            if (hit.collider.gameObject.tag == "ObjectItem")
            {
                Item icon = hit.collider.gameObject.GetComponent<Item>();
                if (icon != null && icon.item != null)
                {
                    Icon = icon.item.sprite;
                    if (Input.GetKeyDown("f"))
                    {
                        icon.item._id = hit.collider.gameObject;
                        AddItem(icon.item, hit.collider.gameObject);
                    }
                    itemIcon = true;
                }
            }
            else
            {
                itemIcon = false;
            }
            if (hit.collider.gameObject.tag == "ShopBox")
            {
                if (Input.GetKeyDown("f"))
                {
                    mtp.init();
                    showShop = !showShop;
                    CalcElement();
                    ShopGm = hit.collider.gameObject;

                    if (GameControl.ActivateGUI && !showShop)
                    {
                        GameControl.ActivateGUI = false;
                    }
                    else
                    {
                        GameControl.ActivateGUI = true;
                        showGUI = false;
                    }

                    GetComponent<AudioSource>().clip = soundFx.invOpen;
                    GetComponent<AudioSource>().Play();
                    UpdateCursorState();
                }
            }
            else
            {
                if (!showGUI)
                {
                    showShop = false;
                    GameControl.ActivateGUI = false;
                    UpdateCursorState();
                }
            }
        }
        else
        {
            itemIcon = false;
        }
    }


    public void CloseInventory()
    {
        showGUI = false;
        showShop = false;
        GameControl.ActivateGUI = false;
        UpdateCursorState();
    }

    void Update()
    {
        if (!showGUI && !showShop)
        {
            objectItemAdd();
        }

        keyStat();
        if ((showGUI || showShop) && Input.GetKeyDown(KeyCode.Escape))
        {
            showGUI = false;
            showShop = false;
            GameControl.ActivateGUI = false;
            UpdateCursorState();
        }
    }

}