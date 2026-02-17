using System.Collections;
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
    private bool ready = false;

    public bool IsSold => sold;

    public void Setup(ItemSO newItem, int overridePrice = -1)
    {
        item = newItem;
        priceOverride = overridePrice;
        sold = false;

        // If no item, hide stand 
        gameObject.SetActive(item != null);

       
        if (ready) RefreshUI();
    }

    public void SetPriceMultiplier(float m) => priceMultiplier = Mathf.Max(0.1f, m);

    private void OnEnable()
    {
        
        StartCoroutine(WaitForPlayerManager());
    }

    IEnumerator WaitForPlayerManager()
    {
        ready = false;

       
        if (promptUI) promptUI.SetActive(false);

        
        while (PlayerManager.instance == null ||
               PlayerManager.instance.player == null ||
               PlayerManager.instance.inventory == null ||
               PlayerManager.instance.wallet == null)
        {
            yield return null;
        }

        player = PlayerManager.instance.player.transform;
        ready = true;

        RefreshUI();
    }

    void Update()
    {
        if (!ready || !player || sold || !item)
        {
            if (promptUI) promptUI.SetActive(false);
            return;
        }

        bool close = Vector3.Distance(transform.position, player.position) <= interactRadius;
        if (promptUI) promptUI.SetActive(close);

        if (close && Input.GetKeyDown(KeyCode.E))
            TryBuy();
    }

    public void RefreshUI()
    {
        if (!item)
        {
            if (nameText) nameText.SetText("");
            if (priceText) priceText.SetText("");
            return;
        }

        if (nameText) nameText.SetText(item.displayName);

        // Only show price if PM is ready
        int price = GetCurrentPrice();
        if (priceText) priceText.SetText($"$ {price}");
    }

    int GetCurrentPrice()
    {
        var pm = PlayerManager.instance;
        if (pm == null || item == null) return 0;
        if (pm.inventory == null) return Mathf.RoundToInt(item.basePrice * priceMultiplier);

        int owned = pm.inventory.GetCount(item.id);
        int basePrice = (priceOverride >= 0) ? priceOverride : item.GetPriceForNext(owned);
        return Mathf.RoundToInt(basePrice * priceMultiplier);
    }

    void TryBuy()
    {
        var pm = PlayerManager.instance;
        if (!pm || !item || pm.inventory == null || pm.wallet == null)
        {
            Debug.LogWarning("[ShopItemStand] Missing PlayerManager/wallet/inventory.");
            return;
        }

        // Don’t let the player buy again if they already own it
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
        FindObjectOfType<ItemEffectsManager>()?.Reapply();

        sold = true;
        gameObject.SetActive(false);
    }
}
