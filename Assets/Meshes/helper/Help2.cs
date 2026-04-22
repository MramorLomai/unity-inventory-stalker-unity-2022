using UnityEngine;

public class Help2 : MonoBehaviour
{
    [SerializeField] private float healthBonus = 300f;

    void Start()
    {
        ActorStatus.heartLevel += healthBonus;
        Destroy(gameObject);
    }
}