using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class ItemDefinition
{
    public string id = "medkit";
    public string displayName = "Medkit";
    [TextArea] public string description = "Heals you in-game.";
    public Sprite icon;
    public int price = 100;
    public bool stackable = true;
    public int maxStack = 99;
    public int initialStock = -1; // -1 = unlimited stock in shop
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

    public void Add(ItemDefinition def, int amount = 1)
    {
        if (def == null || amount <= 0) return;

        if (def.stackable)
        {
            var stack = items.Find(s => s.item == def);
            if (stack == null)
            {
                items.Add(new ItemStack { item = def, count = Mathf.Min(amount, def.maxStack) });
            }
            else
            {
                stack.count = Mathf.Min(stack.count + amount, def.maxStack);
            }
        }
        else
        {
            for (int i = 0; i < amount; i++)
                items.Add(new ItemStack { item = def, count = 1 });
        }
        onChanged.Invoke();
    }
}
