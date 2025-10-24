using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class ShopHorizontalUI : MonoBehaviour
{
    [Header("Refs")]
    public ShopCatalog catalog;            
    public RectTransform contentParent;    // ScrollView/Viewport/Content
    public SimpleCardRow cardPrefab;       // Card_Simple prefab

    private readonly List<SimpleCardRow> pool = new();
    private PlayerManager pm;

    void OnEnable()
    {
        pm = PlayerManager.instance;
        if (!catalog) catalog = GetComponent<ShopCatalog>();

        EnsureStockArray();
        BuildPool(catalog.items?.Length ?? 0);
        RefreshAll();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }

    void EnsureStockArray()
    {
        if (catalog == null || catalog.items == null) return;
        if (catalog.stockRuntime == null || catalog.stockRuntime.Length != catalog.items.Length)
        {
            catalog.stockRuntime = new int[catalog.items.Length];
            for (int i = 0; i < catalog.items.Length; i++) catalog.stockRuntime[i] = -1; // unlimited by default
        }
    }

    void BuildPool(int needed)
    {
        if (!contentParent || !cardPrefab) return;

        for (int i = pool.Count; i < needed; i++)
        {
            var card = Instantiate(cardPrefab, contentParent);
            Wire(card);
            pool.Add(card);
        }
        for (int i = needed; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);
    }

    void Wire(SimpleCardRow card)
    {
        card.actionButton.onClick.RemoveAllListeners();
        card.actionButton.onClick.AddListener(() => Buy(card.index));
    }

    void Buy(int index)
    {
        pm = PlayerManager.instance;
        if (!pm || catalog == null || catalog.items == null) return;
        if (index < 0 || index >= catalog.items.Length) return;

        var item = catalog.items[index];
        if (!item || !catalog.HasStock(index)) return;

        int owned = pm.inventory ? pm.inventory.GetCount(item.id) : 0;
        int price = item.GetPriceForNext(owned);
        if (!pm.wallet || !pm.wallet.Spend(price)) return;

        pm.inventory.Add(item, 1);
        item.OnPurchased(pm.GetContext(), pm.inventory.GetCount(item.id));
        catalog.ConsumeStock(index);

        // apply passive effects immediately (optional)
        FindObjectOfType<ItemEffectsManager>()?.Reapply();

        RefreshAll();
    }

    public void RefreshAll()
    {
        if (catalog == null || catalog.items == null) return;
        pm = PlayerManager.instance;

        int n = catalog.items.Length;
        BuildPool(n);

        for (int i = 0; i < pool.Count; i++)
        {
            var card = pool[i];
            if (i >= n || catalog.items[i] == null)
            {
                card.gameObject.SetActive(false);
                continue;
            }

            var def = catalog.items[i];
            card.index = i;
            card.gameObject.SetActive(true);

            if (card.icon)      card.icon.sprite = def.icon;
            if (card.nameText)  card.nameText.text = def.displayName;

            // Button label shows price ($X)
            int owned = pm?.inventory ? pm.inventory.GetCount(def.id) : 0;
            int price = def.GetPriceForNext(owned);
            if (card.actionLabel) card.actionLabel.text = $"$ {price}";

            bool canAfford = pm?.wallet && pm.wallet.amount >= price;
            bool inStock = catalog.HasStock(i);
            if (card.actionButton) card.actionButton.interactable = canAfford && inStock;
        }
    }
}
