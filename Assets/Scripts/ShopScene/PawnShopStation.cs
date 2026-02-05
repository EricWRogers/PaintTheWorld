using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PawnShopStation : MonoBehaviour
{
    [Header("World UI")]
    public GameObject panelRoot;          // the panel to enable
    public RectTransform contentParent;   // where rows spawn
    public PawnShopRow rowPrefab;         // row prefab
    public KioskListNavigator navigator;  
    public KioskToast toast;              
    public Transform cameraTarget;        // where camera moves

    [Header("Rules")]
    public bool usedThisVisit = false;

    private PlayerManager pm;
    private readonly List<PawnShopRow> rows = new();
    private readonly List<ItemSO> ownedUnique = new();

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
    }

    public void Open()
    {
        pm = PlayerManager.instance;
        if (!pm || !pm.inventory) return;

        if (usedThisVisit)
        {
            toast?.Show("Pawn shop already used this visit.");
            return;
        }

        KioskCameraController.I.EnterKiosk(cameraTarget, () =>
        {
            panelRoot.SetActive(true);
            BuildList();
        });
    }

    void Update()
    {
        if (panelRoot && panelRoot.activeSelf && Input.GetKeyDown(KeyCode.Backspace))
            Close();
    }

    public void Close()
    {
        if (panelRoot) panelRoot.SetActive(false);
        KioskCameraController.I.ExitKiosk(null);
    }

    void BuildList()
    {
        ClearRows();
        ownedUnique.Clear();

        
        foreach (var stack in pm.inventory.items)
        {
            if (stack == null || stack.item == null) continue;
            if (stack.count <= 0) continue;

            // avoid duplicates by ID
            bool already = ownedUnique.Exists(i => i.id == stack.item.id);
            if (!already) ownedUnique.Add(stack.item);
        }

        if (ownedUnique.Count == 0)
        {
            toast?.Show("No items to swap.");
            return;
        }

       
        var selectable = new List<SelectableRow>();

        for (int i = 0; i < ownedUnique.Count; i++)
        {
            var item = ownedUnique[i];
            var row = Instantiate(rowPrefab, contentParent);
            rows.Add(row);

            int count = pm.inventory.GetCount(item.id);
            row.Bind(item, count);

            int index = i;
            row.selectable.Bind(() => TrySwap(index), true);
            selectable.Add(row.selectable);
        }

        navigator.SetRows(selectable);
    }

    void TrySwap(int index)
    {
        if (usedThisVisit) { toast?.Show("Already used."); return; }
        if (index < 0 || index >= ownedUnique.Count) return;

        var chosen = ownedUnique[index];
        if (!chosen) return;

        // Roll replacement of same rarity
        var db = ItemDatabase.Load();
        if (!db) { toast?.Show("ItemDatabase missing."); return; }

        var candidates = new List<ItemSO>();
        foreach (var it in db.items)
        {
            if (!it) continue;
            if (it.rarity != chosen.rarity) continue;

            // prevent swapping into exact same item
            if (it.id == chosen.id) continue;

            candidates.Add(it);
        }

        if (candidates.Count == 0)
        {
            toast?.Show("No swap candidates for this rarity.");
            return;
        }

        var replacement = candidates[Random.Range(0, candidates.Count)];

        // Remove 1 of chosen and add 1 of replacement
        bool removed = pm.inventory.Consume(chosen.id, 1);
        if (!removed)
        {
            toast?.Show("Could not remove item.");
            return;
        }

        pm.inventory.Add(replacement, 1);
        replacement.OnPurchased(pm.GetContext(), pm.inventory.GetCount(replacement.id));

        usedThisVisit = true;
        toast?.Show($"Swapped {chosen.displayName} → {replacement.displayName}");
        BuildList();
    }

    void ClearRows()
    {
        for (int i = 0; i < rows.Count; i++)
            if (rows[i]) Destroy(rows[i].gameObject);
        rows.Clear();
    }
}
