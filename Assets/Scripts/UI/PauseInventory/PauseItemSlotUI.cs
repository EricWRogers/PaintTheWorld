using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;
    

    private ItemSO item;
    private bool owned;
    private PauseItemTooltip tooltip;

    public void Setup(ItemSO newItem, bool isOwned, PauseItemTooltip tooltipRef)
    {
        item = newItem;
        owned = isOwned;
        tooltip = tooltipRef;

        if (iconImage != null)
        {
            iconImage.sprite = item != null ? item.icon : null;
            iconImage.enabled = item != null && item.icon != null;
            iconImage.color = owned ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (item == null || tooltip == null) return;
        tooltip.Show(item.displayName, item.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip == null) return;
        tooltip.Hide();
    }
}