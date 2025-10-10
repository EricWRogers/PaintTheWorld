using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Refs")]
    public ShopCatalog catalog;

    [System.Serializable]
    public class ItemRow
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text descText;
        public TMP_Text priceText;
        public TMP_Text stockText;
        public Button buyButton;
        public Image rarityStripe; // optional: tint by rarity
        
    }

    [Header("UI Rows (size >= catalog.items length)")]
    public ItemRow[] rows;

    private void Awake()
    {
        if (!catalog) catalog = GetComponent<ShopCatalog>();
    }

    private void OnEnable()
    {
        var pm = PlayerManager.instance;
        if (pm && pm.wallet && pm.wallet.changed != null)
            pm.wallet.changed.AddListener(OnWalletChanged);
        RefreshAll();
    }

    private void OnDisable()
    {
        var pm = PlayerManager.instance;
        if (pm && pm.wallet && pm.wallet.changed != null)
            pm.wallet.changed.RemoveListener(OnWalletChanged);
    }

    private void Start()
    {
        BindButtons();
        RefreshAll();
    }

    private void OnWalletChanged(int amt) => RefreshAll();

    public void BindButtons()
    {
        if (rows == null) return;
        for (int i = 0; i < rows.Length; i++)
        {
            int idx = i;
            if (rows[i]?.buyButton != null)
            {
                rows[i].buyButton.onClick.RemoveAllListeners();
                rows[i].buyButton.onClick.AddListener(() => Buy(idx));
            }
        }
    }

    public void Buy(int index)
    {
        var pm = PlayerManager.instance;
        if (!pm || catalog == null || catalog.items == null) return;
        if (index < 0 || index >= catalog.items.Length) return;

        var item = catalog.items[index];
        if (!item) return;
        if (!catalog.HasStock(index)) return;

        int currentCount = pm.inventory.GetCount(item.id);
        int price = item.GetPriceForNext(currentCount);

        if (!pm.wallet.Spend(price)) return;

        pm.inventory.Add(item, 1);
        item.OnPurchased(pm.GetContext(), pm.inventory.GetCount(item.id));
        catalog.ConsumeStock(index);

        RefreshAll();
    }

    private void RefreshAll()
    {
        var pm = PlayerManager.instance;
        if (!pm || rows == null || catalog == null || catalog.items == null) return;

        int count = Mathf.Min(rows.Length, catalog.items.Length);

        for (int i = 0; i < count; i++)
        {
            var item = catalog.items[i];
            var row = rows[i];
            if (row == null) { Debug.LogWarning($"[ShopUI] Row {i} is null"); continue; }

            if (!item)
            {
                SetRow(row, false);
                continue;
            }

            // Icon / Texts (guard every field)
            if (row.icon) row.icon.sprite = item.icon; else Debug.LogWarning($"[ShopUI] row[{i}].icon not assigned");
            if (row.nameText) row.nameText.text = item.displayName; else Debug.LogWarning($"[ShopUI] row[{i}].nameText not assigned");
            if (row.descText) row.descText.text = item.description; else Debug.LogWarning($"[ShopUI] row[{i}].descText not assigned");

            int owned = pm.inventory.GetCount(item.id);
            int price = item.GetPriceForNext(owned);
            if (row.priceText) row.priceText.text = $"$ {price}"; else Debug.LogWarning($"[ShopUI] row[{i}].priceText not assigned");

            bool inStock = catalog.HasStock(i);
            if (row.stockText)
            {
                int s = catalog.GetStock(i);
                row.stockText.text = (s < 0) ? "∞" : $"Stock: {s}";
            }
            else Debug.LogWarning($"[ShopUI] row[{i}].stockText not assigned");

            if (row.buyButton)
                row.buyButton.interactable = inStock && pm.wallet.amount >= price;
            else
                Debug.LogWarning($"[ShopUI] row[{i}].buyButton not assigned");

            if (row.rarityStripe)
            {
                row.rarityStripe.color = item.rarity switch
                {
                    ItemRarity.Common => new Color(0.80f, 0.80f, 0.80f),
                    ItemRarity.Rare   => new Color(0.45f, 0.75f, 1.00f),
                    ItemRarity.Epic   => new Color(0.80f, 0.55f, 1.00f),
                    _ => Color.white
                };
            }
        }

        // Hide extras
        for (int i = count; i < rows.Length; i++) SetRow(rows[i], false);
    }

    private void SetRow(ItemRow row, bool on)
    {
        if (row == null) return;
        if (row.buyButton) row.buyButton.interactable = false;
        if (row.icon) row.icon.enabled = on;
        if (row.nameText) row.nameText.enabled = on;
        if (row.descText) row.descText.enabled = on;
        if (row.priceText) row.priceText.enabled = on;
        if (row.stockText) row.stockText.enabled = on;
        if (row.rarityStripe) row.rarityStripe.enabled = on;
    }
}
