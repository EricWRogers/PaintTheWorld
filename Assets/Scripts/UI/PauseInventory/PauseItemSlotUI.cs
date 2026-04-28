using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class PauseItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image iconImage;

    [Header("Stack Count UI")]
    public TextMeshProUGUI stackCountText;
    public GameObject stackCountBackground;

    private ItemSO item;
    private bool owned;
    private int stackCount;
    private PauseItemTooltip tooltip;

    public void Setup(ItemSO newItem, bool isOwned, int count, PauseItemTooltip tooltipRef)
    {
        item = newItem;
        owned = isOwned;
        stackCount = count;
        tooltip = tooltipRef;

        if (iconImage != null)
        {
            iconImage.sprite = item != null ? item.icon : null;
            iconImage.enabled = item != null && item.icon != null;
            iconImage.color = owned ? Color.white : new Color(0.4f, 0.4f, 0.4f, 1f);
        }

        UpdateStackCountUI();
    }

    private void UpdateStackCountUI()
    {
        bool showStackUI = stackCount > 0;

        if (stackCountText != null)
        {
            stackCountText.gameObject.SetActive(showStackUI);

            if (showStackUI)
            {
                stackCountText.text = stackCount.ToString();
            }
        }

        if (stackCountBackground != null)
        {
            stackCountBackground.SetActive(showStackUI);
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