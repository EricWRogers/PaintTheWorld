using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopItemRow : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public TMP_Text descText;
    public TMP_Text priceText;
    public TMP_Text stockText;
    public Button buyButton;
    public Image rarityStripe;

    [HideInInspector] public int index; 
}

