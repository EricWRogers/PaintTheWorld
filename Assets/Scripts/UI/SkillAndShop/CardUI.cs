using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CardUI : MonoBehaviour
{
    [Header("UI")]
    public Image icon;
    public TMP_Text nameText;
    public Image underline;
    public Button actionButton;
    public TMP_Text actionLabel;
    public TMP_Text priceOrCost;   // small label near the button
    public TMP_Text stockOrLevel;  // small label under price/cost

    [HideInInspector] public int dataIndex = -1;
}
