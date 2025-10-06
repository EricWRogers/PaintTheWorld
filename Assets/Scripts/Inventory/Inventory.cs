using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable] public class InventoryChangedEvent : UnityEvent {}

public class Inventory : MonoBehaviour
{
    [Serializable] public class ItemStack { public ItemSO item; public int count; }

    public List<ItemStack> items = new();
    public InventoryChangedEvent onChanged;

    void Awake() { if (onChanged == null) onChanged = new InventoryChangedEvent(); }

    public int GetCount(string id)
    {
        int total = 0;
        foreach (var s in items) if (s?.item && s.item.id == id) total += s.count;
        return total;
    }

    public void Add(ItemSO def, int amount = 1)
    {
        if (!def || amount <= 0) return;

        if (def.stackable)
        {
            var stack = items.Find(s => s.item && s.item.id == def.id);
            if (stack == null)
                items.Add(new ItemStack { item = def, count = Mathf.Min(amount, Mathf.Max(1, def.maxStack)) });
            else
                stack.count = Mathf.Min(stack.count + amount, Mathf.Max(1, def.maxStack));
        }
        else
        {
            for (int i = 0; i < amount; i++)
                items.Add(new ItemStack { item = def, count = 1 });
        }
        onChanged.Invoke();
    }

    public bool Consume(string id, int amount = 1)
    {
        int need = amount;
        for (int i = items.Count - 1; i >= 0 && need > 0; i--)
        {
            var s = items[i];
            if (s?.item == null || s.item.id != id) continue;
            int take = Mathf.Min(s.count, need);
            s.count -= take; need -= take;
            if (s.count <= 0) items.RemoveAt(i);
        }
        onChanged.Invoke();
        return need == 0;
    }
}
