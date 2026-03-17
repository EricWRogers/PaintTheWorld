using UnityEngine;
using TMPro;
using SuperPupSystems.Helper;

public class MoneyCounterUI : MonoBehaviour
{
    [Header("References")]
    public Currency moneyLogic;
    public TextMeshProUGUI moneyText;

    [Header("Settings")]
    public string prefix = "Money: ";

    void Update()
    {
        if (moneyLogic != null && moneyText != null)
        {
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        moneyText.text = prefix + moneyLogic.amount.ToString();
    }
}
