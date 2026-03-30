using System.Collections.Generic;
using UnityEngine;

public class PauseInventoryDisplay : MonoBehaviour
{
    [Header("References")]
    public Transform gridContainer;
    public PauseItemSlotUI slotPrefab;
    public PauseItemTooltip tooltip;
    public ItemDatabase database;

    private readonly List<PauseItemSlotUI> liveSlots = new();

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        ClearSlots();

        if (database == null)
            database = ItemDatabase.Load();

        if (database == null || gridContainer == null || slotPrefab == null)
        {
            Debug.LogWarning("PauseInventoryDisplay missing references.");
            return;
        }

        Inventory inv = PlayerManager.instance != null ? PlayerManager.instance.inventory : null;

        foreach (var item in database.items)
        {
            if (item == null) continue;

            bool owned = inv != null && inv.GetCount(item.id) > 0;

            PauseItemSlotUI slot = Instantiate(slotPrefab, gridContainer);
            slot.Setup(item, owned, tooltip);
            liveSlots.Add(slot);
        }
    }

    void ClearSlots()
    {
        foreach (var slot in liveSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        liveSlots.Clear();
    }
}