using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopUI : MonoBehaviour
{
    [Header("Refs")]
    public ShopCatalog catalog;
    public Currency wallet;
    public Inventory inventory;

    [System.Serializable]
    public class ItemRow
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text descText;
        public TMP_Text priceText;
        public TMP_Text stockText;
        public Button buyButton;
    }

    [Header("UI Rows (create same count as items)")]
    public ItemRow[] rows;

    private void Awake()
    {
        if (!catalog) catalog = FindObjectOfType<ShopCatalog>();
        if (!wallet) wallet = FindObjectOfType<Currency>();
        if (!inventory) inventory = FindObjectOfType<Inventory>();
    }

    private void OnEnable()
    {
        if (wallet && wallet.changed != null) wallet.changed.AddListener(OnWalletChanged);
        RefreshAll();
    }

    private void OnDisable()
    {
        if (wallet && wallet.changed != null) wallet.changed.RemoveListener(OnWalletChanged);
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

    private void Buy(int index)
    {
        if (catalog == null || wallet == null || inventory == null) return;
        if (catalog.items == null || index < 0 || index >= catalog.items.Length) return;

        var def = catalog.items[index];
        if (def == null) return;

        if (!catalog.HasStock(index)) return;   // out of stock
        if (wallet.amount < def.price) return;  // cannot afford
        if (!PlayerManager.instance.wallet.Spend(def.price)) return;

        PlayerManager.instance.inventory.Add(def, 1);
        catalog.ConsumeStock(index);
        RefreshAll();
    }

    private void RefreshAll()
    {
        if (rows == null || catalog == null || catalog.items == null) return;

        int count = Mathf.Min(rows.Length, catalog.items.Length);
        for (int i = 0; i < count; i++)
        {
            var def = catalog.items[i];
            var row = rows[i];
            if (def == null || row == null) continue;

            if (row.icon) row.icon.sprite = def.icon;
            if (row.nameText) row.nameText.text = def.displayName;
            if (row.descText) row.descText.text = def.description;
            if (row.priceText) row.priceText.text = $"$ {def.price}";

            bool inStock = catalog.HasStock(i);
            if (row.stockText)
            {
                int s = catalog.stockRuntime[i];
                row.stockText.text = (s < 0) ? "∞" : $"Stock: {s}";
            }

            if (row.buyButton)
            {
                bool canAfford = wallet && wallet.amount >= def.price;
                row.buyButton.interactable = canAfford && inStock;
            }
        }
    }
}
