using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopHorizontalUI : MonoBehaviour
{
    [Header("Refs")]
    public ShopCatalog catalog;                  
    public RectTransform contentParent;          // ScrollView/Viewport/Content
    public SimpleCardRow cardPrefab;             // Card_Simple prefab

    private readonly List<SimpleCardRow> pool = new();
    private bool walletWired = false;

    void OnEnable()
    {
        StartCoroutine(InitAndRefreshWhenReady());
    }

    IEnumerator InitAndRefreshWhenReady()
    {
        // Wait until PlayerManager is spawned & ready for the popup
        for (int i = 0; i < 180; i++) 
        {
            if (PlayerManager.instance && PlayerManager.instance.IsReady) break;
            yield return null;
        }

        if (!catalog) catalog = GetComponent<ShopCatalog>();
        EnsureStockArray();

        // (re)wire wallet change event now that PM exists
        var pm = PlayerManager.instance;
        if (pm && pm.wallet && pm.wallet.changed != null && !walletWired)
        {
            pm.wallet.changed.AddListener(OnWalletChanged);
            walletWired = true;
        }

        BuildPool(catalog.items?.Length ?? 0);
        RefreshAll();

        Canvas.ForceUpdateCanvases();
        if (contentParent) LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }

    void OnDisable()
    {
        var pm = PlayerManager.instance;
        if (walletWired && pm && pm.wallet && pm.wallet.changed != null)
            pm.wallet.changed.RemoveListener(OnWalletChanged);
        walletWired = false;
    }

    void OnWalletChanged(int _) => RefreshAll();

    void EnsureStockArray()
    {
        if (catalog == null || catalog.items == null) return;
        if (catalog.stockRuntime == null || catalog.stockRuntime.Length != catalog.items.Length)
        {
            catalog.stockRuntime = new int[catalog.items.Length];
            for (int i = 0; i < catalog.items.Length; i++)
                catalog.stockRuntime[i] = -1; // unlimited by default
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
        // Re-resolve PM at click-time (popup-safe)
        var pm = PlayerManager.instance;
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

        FindObjectOfType<ItemEffectsManager>()?.Reapply();
        RefreshAll();
    }

    public void RefreshAll()
    {
        if (catalog == null || catalog.items == null) return;

        int n = catalog.items.Length;
        BuildPool(n);

        var pm = PlayerManager.instance;

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

            if (card.icon && def != null && def.icon != null)
                card.icon.sprite = def.icon;
            if (card.nameText)
                card.nameText.text = def.displayName;

            int owned = pm?.inventory ? pm.inventory.GetCount(def.id) : 0;
            int price = def.GetPriceForNext(owned);
            if (card.actionLabel)
                card.actionLabel.text = $"$ {price}";

            bool canAfford = (pm?.wallet != null) && pm.wallet.amount >= price;
            bool inStock  = catalog.HasStock(i);
            if (card.actionButton)
                card.actionButton.interactable = canAfford && inStock;
        }
    }
}
