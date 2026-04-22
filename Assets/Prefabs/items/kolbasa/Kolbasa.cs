using UnityEngine;

public class Kolbasa : MonoBehaviour
{
    [SerializeField] private float healthBonus = 20f;

    void Start()
    {
        ActorStatus.heartLevel += healthBonus;
        Destroy(gameObject);
    }
}