using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PawnShopStation : MonoBehaviour
{
    [Header("UI")]
    public GameObject panelRoot;
    public RectTransform contentParent;
    public PawnShopRow rowPrefab;
    public KioskListNavigator navigator;
    public TMP_Text toastText;

    [Header("Camera")]
    public Transform cameraTarget;

    [Header("Rules")]
    public bool usedThisVisit;

    private PlayerManager pm;
    private readonly List<PawnShopRow> liveRows = new();
    private readonly List<ItemSO> ownedUnique = new();

    void Awake()
    {
        if (panelRoot) panelRoot.SetActive(false);
        ShowToast("");
    }

    public void Open()
    {
        pm = PlayerManager.instance;
        if (!pm || pm.inventory == null)
        {
            ShowToast("PlayerManager/Inventory missing.");
            return;
        }

        if (usedThisVisit)
        {
            ShowToast("Pawn shop already used this visit.");
            return;
        }

        KioskCameraController.I.EnterKiosk(cameraTarget, () =>
        {
            panelRoot.SetActive(true);
            navigator.active = true;
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
        navigator.active = false;
        if (panelRoot) panelRoot.SetActive(false);
        KioskCameraController.I.ExitKiosk();
    }

    void BuildList()
    {
        ClearRows();
        ownedUnique.Clear();
        ShowToast("");

        // build unique list from inventory
        foreach (var stack in pm.inventory.items)
        {
            if (stack == null || stack.item == null) continue;
            if (stack.count <= 0) continue;

            bool exists = ownedUnique.Exists(x => x.id == stack.item.id);
            if (!exists) ownedUnique.Add(stack.item);
        }

        if (ownedUnique.Count == 0)
        {
            ShowToast("No items in inventory to swap.");
            navigator.SetRows(new List<SelectableRow>());
            return;
        }

        var selectables = new List<SelectableRow>();

        for (int i = 0; i < ownedUnique.Count; i++)
        {
            var item = ownedUnique[i];
            int count = pm.inventory.GetCount(item.id);

            var row = Instantiate(rowPrefab, contentParent);
            liveRows.Add(row);

            row.Bind(item, count);

            int idx = i;
            row.selectable.Bind(() => TrySwap(idx), true);
            selectables.Add(row.selectable);
        }

        navigator.SetRows(selectables);
    }

    void TrySwap(int index)
    {
        if (usedThisVisit) { ShowToast("Already used."); return; }
        if (index < 0 || index >= ownedUnique.Count) return;

        var chosen = ownedUnique[index];
        if (!chosen) return;

        var db = ItemDatabase.Load();
        if (!db)
        {
            ShowToast("ItemDatabase not found (Resources path issue).");
            return;
        }

        var candidates = new List<ItemSO>();
        foreach (var it in db.items)
        {
            if (!it) continue;
            if (it.rarity != chosen.rarity) continue;
            if (it.id == chosen.id) continue; // prevent same
            candidates.Add(it);
        }

        if (candidates.Count == 0)
        {
            ShowToast("No candidates of same rarity.");
            return;
        }

        var replacement = candidates[Random.Range(0, candidates.Count)];

        bool removed = pm.inventory.Consume(chosen.id, 1);
        if (!removed)
        {
            ShowToast("Could not remove item from inventory.");
            return;
        }

        pm.inventory.Add(replacement, 1);
        replacement.OnPurchased(pm.GetContext(), pm.inventory.GetCount(replacement.id));

        usedThisVisit = true;
        ShowToast($"Swapped {chosen.displayName} → {replacement.displayName}");
        BuildList();
    }

    void ClearRows()
    {
        foreach (var r in liveRows)
            if (r) Destroy(r.gameObject);
        liveRows.Clear();
    }

    void ShowToast(string msg)
    {
        if (toastText) toastText.text = msg;
    }
}
