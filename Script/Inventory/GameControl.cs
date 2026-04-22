using UnityEngine;

public class GameControl : MonoBehaviour
{
    public static bool ActivateGUI = false;
    public static float actorMoney = 5000;

    void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 100, 55), "actorMoney:" + actorMoney.ToString());
        GUI.Label(new Rect(10, 30, 100, 55), "STATUS:" + ActivateGUI.ToString());
    }
}