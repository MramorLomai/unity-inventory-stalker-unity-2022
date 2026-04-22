using UnityEngine;

public class Bred : MonoBehaviour
{
    [SerializeField] private float healthBonus = 10f;

    void Start()
    {
        ActorStatus.heartLevel += healthBonus;
        Destroy(gameObject);
    }
}