using UnityEngine;

public class Help1 : MonoBehaviour
{
    [SerializeField] private float healthBonus = 500f;

    void Start()
    {
        ActorStatus.heartLevel += healthBonus;
        Destroy(gameObject);
    }
}