using UnityEngine;

public class Help3 : MonoBehaviour
{
    [SerializeField] private float healthBonus = 100f;

    void Start()
    {
        ActorStatus.heartLevel += healthBonus;
        Destroy(gameObject);
    }
}