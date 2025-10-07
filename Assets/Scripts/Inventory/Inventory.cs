


using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class ItemDefinition
{
    public string id = "medkit";            // <-- must be unique across items
    public string displayName = "Medkit";
    [TextArea] public string description = "Heals you in-game.";
    public Sprite icon;
    public int price = 100;
    public bool stackable = true;
    public int maxStack = 99;
    public int initialStock = -1; // -1 = unlimited in shop
}


[System.Serializable]
public class ItemStack
{
    public ItemDefinition item;
    public int count;
}


[System.Serializable] public class InventoryChangedEvent : UnityEvent {}


public class Inventory : MonoBehaviour
{
    public List<ItemStack> items = new List<ItemStack>();
    public InventoryChangedEvent onChanged;


    private void Awake()
    {
        if (onChanged == null) onChanged = new InventoryChangedEvent();
    }


    /// <summary>Adds amount of a definition. Merges by item.id
    public void Add(ItemDefinition def, int amount = 1)
    {
        if (def == null || amount <= 0) return;


        if (def.stackable)
        {
            // Merge by id so different instances of the same item still stack.
            var stack = items.Find(s => s.item != null && s.item.id == def.id);
            if (stack == null)
            {
                items.Add(new ItemStack
                {
                    item = def,
                    count = Mathf.Min(amount, Mathf.Max(1, def.maxStack))
                });
            }
            else
            {
                int cap = Mathf.Max(1, def.maxStack);
                stack.count = Mathf.Min(stack.count + amount, cap);
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
                items.Add(new ItemStack { item = def, count = 1 });
        }


        onChanged.Invoke();
    }


    /// <summary>Total count across all stacks for an item id.</summary>
    public int GetCount(string id)
    {
        if (string.IsNullOrEmpty(id)) return 0;
        int total = 0;
        foreach (var s in items)
            if (s?.item != null && s.item.id == id)
                total += s.count;
        return total;
    }


    /// <summary>Consumes up to amount from stacks with this id. Returns true if fully consumed.</summary>
    public bool Consume(string id, int amount = 1)
    {
        if (string.IsNullOrEmpty(id) || amount <= 0) return false;


        int need = amount;
        for (int i = items.Count - 1; i >= 0 && need > 0; i--)
        {
            var s = items[i];
            if (s?.item == null || s.item.id != id) continue;


            int take = Mathf.Min(s.count, need);
            s.count -= take;
            need -= take;


            if (s.count <= 0) items.RemoveAt(i);
        }


        if (need == 0) { onChanged.Invoke(); return true; }
       
        onChanged.Invoke();
        return false;
    }


    /// collapse duplicate stacks with the same id
    public void MergeDuplicatesById()
    {
        var map = new Dictionary<string, ItemStack>();
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var s = items[i];
            if (s?.item == null) { items.RemoveAt(i); continue; }
            string id = s.item.id;
            if (string.IsNullOrEmpty(id)) { continue; }


            if (!map.TryGetValue(id, out var existing))
            {
                map[id] = s;
            }
            else
            {
                if (existing.item.stackable)
                {
                    int cap = Mathf.Max(1, existing.item.maxStack);
                    int room = cap - existing.count;
                    int toMove = Mathf.Min(room, s.count);
                    existing.count += toMove;
                    s.count -= toMove;
                }
                if (s.count <= 0) items.RemoveAt(i);
            }
        }
        onChanged.Invoke();
    }


    public void ClearAll()
    {
        items.Clear();
        onChanged.Invoke();
    }
}



