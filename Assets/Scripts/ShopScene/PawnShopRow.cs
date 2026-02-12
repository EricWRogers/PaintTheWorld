using UnityEngine;
using TMPro;

public class PawnShopRow : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text rarityText;
    public TMP_Text countText;
    public SelectableRow selectable;

    public void Bind(ItemSO item, int count)
    {
        if (nameText) nameText.text = item.displayName;
        if (rarityText) rarityText.text = item.rarity.ToString();
        if (countText) countText.text = $"x{count}";
    }
}
