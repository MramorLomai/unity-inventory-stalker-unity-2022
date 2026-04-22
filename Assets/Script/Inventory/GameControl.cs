using UnityEngine;
using UnityEngine.UI;

public class GameControl : MonoBehaviour
{
    public static bool ActivateGUI = false;
    public static float actorMoney = 5000;

    [Header("UI References")]
    [SerializeField] private Text moneyText;
    [SerializeField] private Text statusText;

    private void Start()
    {
        if (moneyText == null)
            moneyText = GameObject.Find("Text (MoneyText)")?.GetComponent<Text>();

        if (statusText == null)
            statusText = GameObject.Find("Text (StatusText)")?.GetComponent<Text>();
    }

    private void Update()
    {
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (moneyText != null)
            moneyText.text = "actorMoney: " + actorMoney.ToString();

        if (statusText != null)
            statusText.text = "STATUS: " + ActivateGUI.ToString();
    }
}