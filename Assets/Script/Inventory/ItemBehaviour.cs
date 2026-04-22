using UnityEngine;
using System.Collections;

public class ItemBehaviour : MonoBehaviour
{
    public Transform cameraTransform;
    public GameObject insertScene;
    private GameObject ins;
    private bool destroy = false;

    void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    public void ObjectActive()
    {
        ins = Instantiate(insertScene, new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
        cameraTransform = Camera.main.transform;
        ins.transform.parent = cameraTransform;
        ins.transform.localPosition = Vector3.zero;
        ins.transform.localEulerAngles = new Vector3(180, 0, 180);
    }

    public void ObjectDeactive()
    {
        if (ins != null)
        {
            ins.SendMessage("showhide", SendMessageOptions.DontRequireReceiver);
            Destroy(ins, 0.3f);
        }
    }
}