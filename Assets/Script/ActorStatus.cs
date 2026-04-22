using UnityEngine;

public class ActorStatus : MonoBehaviour
{
    public static float sleepTime;
    public static float foodTime;
    public static float maxPower;
    public static float heartLevel;
    public static float andArmor;
    public static float atrArmor;
    public static float Armor;

    [SerializeField] private float stdArmor;

    private const float MAX_SLEEP_TIME = 800f;
    private const float MAX_FOOD_TIME = 300f;
    private const float MAX_HEART_LEVEL = 1000f;

    void Start()
    {
        sleepTime = 300f;
        foodTime = 200f;
        heartLevel = 457f;
    }

    void Update()
    {
        sleepTime -= 0.2f * Time.deltaTime;
        foodTime -= 0.8f * Time.deltaTime;
        Armor = stdArmor + andArmor + atrArmor;

        // Ограничение значений
        sleepTime = Mathf.Min(sleepTime, MAX_SLEEP_TIME);
        foodTime = Mathf.Min(foodTime, MAX_FOOD_TIME);
        heartLevel = Mathf.Min(heartLevel, MAX_HEART_LEVEL);

        // Защита от отрицательных значений
        sleepTime = Mathf.Max(sleepTime, 0f);
        foodTime = Mathf.Max(foodTime, 0f);
    }
}