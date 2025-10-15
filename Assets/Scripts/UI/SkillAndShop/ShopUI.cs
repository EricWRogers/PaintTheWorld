using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [Header("Refs")]
    public ShopCatalog catalog;

    [Header("Dynamic List")]
    public Transform contentParent;      
    public ShopItemRow rowPrefab;       

    [Header("Filters")]
    public bool useRarityFilter = true;
    public ItemRarity currentFilter = ItemRarity.Common; // default
    public Toggle allToggle, commonToggle, rareToggle, legendaryToggle; 

    private readonly List<ShopItemRow> pool = new List<ShopItemRow>();

    void Awake()
    {
        if (!catalog) catalog = GetComponent<ShopCatalog>();
        WireFilterToggles();
    }

    void OnEnable()
    {
        var pm = PlayerManager.instance;
        if (pm && pm.wallet && pm.wallet.changed != null)
            pm.wallet.changed.AddListener(OnWalletChanged);

        RefreshAll();
    }

    void OnDisable()
    {
        var pm = PlayerManager.instance;
        if (pm && pm.wallet && pm.wallet.changed != null)
            pm.wallet.changed.RemoveListener(OnWalletChanged);
    }

    void OnWalletChanged(int _) => RefreshAll();

    void WireFilterToggles()
    {
        if (!useRarityFilter) return;

        if (allToggle) allToggle.onValueChanged.AddListener(on =>
        { if (on) { currentFilter = (ItemRarity)(-1); RefreshAll(); } });

        if (commonToggle) commonToggle.onValueChanged.AddListener(on =>
        { if (on) { currentFilter = ItemRarity.Common; RefreshAll(); } });

        if (rareToggle) rareToggle.onValueChanged.AddListener(on =>
        { if (on) { currentFilter = ItemRarity.Rare; RefreshAll(); } });

        if (legendaryToggle) legendaryToggle.onValueChanged.AddListener(on =>
        { if (on) { currentFilter = ItemRarity.Epic; RefreshAll(); } });
    }

    bool PassesFilter(ItemSO item)
    {
        if (!useRarityFilter) return true;
        if ((int)currentFilter == -1) return true; // All
        return item && item.rarity == currentFilter;
    }

    public void RefreshAll()
    {
        if (!catalog || catalog.items == null) return;

       
        var indices = new List<int>();
        for (int i = 0; i < catalog.items.Length; i++)
            if (catalog.items[i] && PassesFilter(catalog.items[i])) indices.Add(i);

        // Ensure enough pooled rows
        for (int i = pool.Count; i < indices.Count; i++)
        {
            var row = Instantiate(rowPrefab, contentParent);
            pool.Add(row);
            BindRow(row);
        }

        // Populate visible rows
        var pm = PlayerManager.instance;
        var inv = pm ? pm.inventory : null;
        var wal = pm ? pm.wallet : null;

        for (int i = 0; i < pool.Count; i++)
        {
            var row = pool[i];
            if (i >= indices.Count)
            {
                row.gameObject.SetActive(false);
                continue;
            }

            int catalogIndex = indices[i];
            var item = catalog.items[catalogIndex];
            row.index = catalogIndex;
            row.gameObject.SetActive(true);

            // Fill visuals
            if (row.icon)      row.icon.sprite = item.icon;
            if (row.nameText)  row.nameText.text = item.displayName;
            if (row.descText)  row.descText.text = item.description;

            int owned = (inv != null) ? inv.GetCount(item.id) : 0;
            int price = item.GetPriceForNext(owned);
            if (row.priceText) row.priceText.text = $"$ {price}";

            bool inStock = catalog.HasStock(catalogIndex);
            if (row.stockText)
            {
                int s = catalog.GetStock(catalogIndex);
                row.stockText.text = (s < 0) ? "∞" : $"Stock: {s}";
            }

            bool canAfford = (wal != null) && wal.amount >= price;
            if (row.buyButton) row.buyButton.interactable = inStock && canAfford;

            if (row.rarityStripe)
            {
                row.rarityStripe.color = item.rarity switch
                {
                    ItemRarity.Common => new Color(0.80f, 0.80f, 0.80f),
                    ItemRarity.Rare   => new Color(0.45f, 0.75f, 1.00f),
                    ItemRarity.Epic   => new Color(0.80f, 0.55f, 1.00f),
                    _                 => Color.white
                };
            }
        }
    }

    void BindRow(ShopItemRow row)
    {
        row.buyButton.onClick.AddListener(() => Buy(row));
    }

    void Buy(ShopItemRow row)
    {
        var pm = PlayerManager.instance;
        if (!pm) return;

        int index = row.index;
        if (index < 0 || index >= catalog.items.Length) return;

        var item = catalog.items[index];
        if (!item || !catalog.HasStock(index)) return;

        int currentCount = pm.inventory ? pm.inventory.GetCount(item.id) : 0;
        int price = item.GetPriceForNext(currentCount);
        if (!pm.wallet || !pm.wallet.Spend(price)) return;

        pm.inventory.Add(item, 1);
        item.OnPurchased(pm.GetContext(), pm.inventory.GetCount(item.id));
        catalog.ConsumeStock(index);

        RefreshAll();
    }

    // Public hooks for filter UI
    public void SetFilterAll()       { currentFilter = (ItemRarity)(-1); RefreshAll(); }
    public void SetFilterCommon()    { currentFilter = ItemRarity.Common; RefreshAll(); }
    public void SetFilterRare()      { currentFilter = ItemRarity.Rare; RefreshAll(); }
    public void SetFilterLegendary() { currentFilter = ItemRarity.Epic; RefreshAll(); }
}
