using UnityEngine;
using TMPro;

public class ShopItemStand : MonoBehaviour
{
    [Header("Item")]
    public ItemSO item;
    public int priceOverride = -1;    // -1 => use item.GetPriceForNext
    public float priceMultiplier = 1f;

    [Header("UI (world-space)")]
    public TMP_Text nameText;
    public TMP_Text priceText;
    public GameObject promptUI;

    [Header("Interact")]
    public float interactRadius = 2.0f;

    private Transform player;
    private bool sold = false;

    public bool IsSold => sold;

    public void Setup(ItemSO newItem, int overridePrice = -1)
    {
        item = newItem;
        priceOverride = overridePrice;
        sold = false;
        RefreshUI();
        gameObject.SetActive(item != null);
    }

    public void SetPriceMultiplier(float m) => priceMultiplier = Mathf.Max(0.1f, m);

    void Start()
    {
        player = PlayerManager.instance ? PlayerManager.instance.player.transform : null;
        RefreshUI();
    }

    void Update()
    {
        if (!player || sold || !item) { if (promptUI) promptUI.SetActive(false); return; }

        bool close = Vector3.Distance(transform.position, player.position) <= interactRadius;
        if (promptUI) promptUI.SetActive(close);

        if (close && Input.GetKeyDown(KeyCode.E))
            TryBuy();
    }

    public void RefreshUI()
    {
        if (!item) return;
        if (nameText)  nameText.SetText(item.displayName);
        if (priceText) priceText.SetText($"$ {GetCurrentPrice()}");
    }

    int GetCurrentPrice()
    {
        var pm = PlayerManager.instance;
        if (!pm || !item) return 0;

        int owned = pm.inventory.GetCount(item.id);
        int basePrice = (priceOverride >= 0) ? priceOverride : item.GetPriceForNext(owned);
        return Mathf.RoundToInt(basePrice * priceMultiplier);
    }

    void TryBuy()
    {
        var pm = PlayerManager.instance;
        if (!pm || !item) return;

        // Don’t let the player buy an item again if they already own it
        if (pm.inventory.GetCount(item.id) > 0)
        {
            Debug.Log("Already owned.");
            return;
        }

        int price = GetCurrentPrice();
        if (!pm.wallet.Spend(price))
        {
            Debug.Log("Not enough currency.");
            return;
        }

        pm.inventory.Add(item, 1);
        item.OnPurchased(pm.GetContext(), pm.inventory.GetCount(item.id));
        FindObjectOfType<ItemEffectsManager>()?.Reapply(); // optional if you use one

        sold = true;
        gameObject.SetActive(false);
    }
}

