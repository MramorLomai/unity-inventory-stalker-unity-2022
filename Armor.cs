using UnityEngine;

public class Armor : MonoBehaviour
{
    [SerializeField] private float armorValue = 60f;

    void Update()
    {
        ActorStatus.andArmor = armorValue;
    }

    public void ObjectActive()
    {
        // Логика активации брони
        ActorStatus.andArmor = armorValue;
    }

    public void ObjectDeactive()
    {
        ActorStatus.andArmor = 0f;
    }
}