using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 
public class ItemsCarouselUI : MonoBehaviour
{
    [Header("Refs")]
    public ShopCatalog catalog;          
    public RectTransform content;        // ItemsSection/.../Viewport/Content
    public CardUI cardPrefab;            

    private readonly List<CardUI> pool = new();
    PlayerManager pm;

    void OnEnable()
    {
        pm = PlayerManager.instance;
        if (!catalog) catalog = GetComponent<ShopCatalog>();
        EnsureStock();
        BuildOrResizePool(catalog.items?.Length ?? 0);
        RefreshAll();

        Canvas.ForceUpdateCanvases();
        if (content) LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    void EnsureStock()
    {
        if (!catalog || catalog.items == null) return;
        if (catalog.stockRuntime == null || catalog.stockRuntime.Length != catalog.items.Length)
        {
            catalog.stockRuntime = new int[catalog.items.Length];
            for (int i = 0; i < catalog.items.Length; i++)
                catalog.stockRuntime[i] = catalog.items[i] ? catalog.items[i].initialStock : -1;
        }
    }

    void BuildOrResizePool(int needed)
    {
        if (!content || !cardPrefab) return;
        for (int i = pool.Count; i < needed; i++)
        {
            var card = Instantiate(cardPrefab, content);
            WireItemCard(card);
            pool.Add(card);
        }
        for (int i = needed; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);
    }

    void WireItemCard(CardUI card)
    {
        card.actionButton.onClick.RemoveAllListeners();
        card.actionButton.onClick.AddListener(() => Buy(card.dataIndex));
    }

    void Buy(int index)
    {
        pm = PlayerManager.instance;
        if (!pm || !catalog || catalog.items == null) return;
        if (index < 0 || index >= catalog.items.Length) return;

        var item = catalog.items[index];
        if (!item || !catalog.HasStock(index)) return;

        int owned = pm.inventory ? pm.inventory.GetCount(item.id) : 0;
        int price = item.GetPriceForNext(owned);
        if (!pm.wallet || !pm.wallet.Spend(price)) return;

        pm.inventory.Add(item, 1);
        item.OnPurchased(pm.GetContext(), pm.inventory.GetCount(item.id));
        catalog.ConsumeStock(index);

        FindObjectOfType<ItemEffectsManager>()?.Reapply();
        RefreshAll();
    }

    public void RefreshAll()
    {
        if (!catalog || catalog.items == null) return;
        pm = PlayerManager.instance;

        for (int i = 0; i < pool.Count; i++)
        {
            var card = pool[i];
            if (i >= catalog.items.Length) { card.gameObject.SetActive(false); continue; }

            var def = catalog.items[i];
            bool active = def != null;
            card.dataIndex = i;
            card.gameObject.SetActive(active);
            if (!active) continue;

            if (card.icon)        card.icon.sprite = def.icon;
            if (card.nameText)    card.nameText.text = def.displayName;
            if (card.actionLabel) card.actionLabel.text = "BUY";

            int owned = pm?.inventory ? pm.inventory.GetCount(def.id) : 0;
            int price = def.GetPriceForNext(owned);
            if (card.priceOrCost)  card.priceOrCost.text = $"$ {price}";

            int s = catalog.GetStock(i);
            if (card.stockOrLevel) card.stockOrLevel.text = (s < 0) ? "Stock: ∞" : $"Stock: {s}";

            bool canAfford = pm?.wallet && pm.wallet.amount >= price;
            bool inStock = catalog.HasStock(i);
            if (card.actionButton) card.actionButton.interactable = canAfford && inStock;
        }
    }
}
