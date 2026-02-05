using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class PawnShopUI : MonoBehaviour
{
    public RectTransform listParent;
    public PawnShopRow rowPrefab;
    public TMP_Text hintLabel;
    public Button closeButton;

    System.Action onSwapped;
    System.Func<bool> chanceProvider;

    PlayerManager pm; ItemDatabase db;

    void Awake()
    {
        if (closeButton) closeButton.onClick.AddListener(()=> gameObject.SetActive(false));
        gameObject.SetActive(false);
    }

    public void Open(System.Action onSwapped, System.Func<bool> chanceProvider)
    {
        this.onSwapped = onSwapped;
        this.chanceProvider = chanceProvider;
        pm = PlayerManager.instance;
        db = ItemDatabase.Load();
        Rebuild();
        gameObject.SetActive(true);
    }

    void Rebuild()
    {
        foreach (Transform t in listParent) Destroy(t.gameObject);
        if (pm == null) return;

        foreach (var st in pm.inventory.items)
        {
            if (st?.item == null) continue;
            var row = Instantiate(rowPrefab, listParent);
            row.icon.sprite = st.item.icon;
            row.nameText.text = $"{st.item.displayName} ({st.item.rarity})";
            row.swapButton.onClick.RemoveAllListeners();
            row.swapButton.onClick.AddListener(()=> TrySwap(st.item));
        }
    }

    void TrySwap(ItemSO giveUp)
    {
       
        if (chanceProvider != null && !chanceProvider.Invoke())
        {
            if (hintLabel) hintLabel.text = "Swap failed.";
            onSwapped?.Invoke();
            gameObject.SetActive(false);
            return;
        }

        // remove one copy
        pm.inventory.Consume(giveUp.id, 1);

        // same-rarity random replacement, don't use same id
        var pool = db.items.Where(i => i && i.rarity == giveUp.rarity && i.id != giveUp.id).ToList();
        ItemSO newItem = pool.Count > 0 ? pool[Random.Range(0, pool.Count)] : giveUp;

        pm.inventory.Add(newItem, 1);
        newItem.OnPurchased(pm.GetContext(), pm.inventory.GetCount(newItem.id));
        FindObjectOfType<ItemEffectsManager>()?.Reapply();

        if (hintLabel) hintLabel.text = $"Received: {newItem.displayName}";
        onSwapped?.Invoke();
        gameObject.SetActive(false);
    }
}
