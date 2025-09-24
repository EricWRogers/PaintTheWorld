
using UnityEngine;

[System.Serializable]
public class ItemEntry { public ItemDefinition item; public int count = 1; }

public class TestLoadout : MonoBehaviour
{
    public Inventory inventory;
    public ItemEntry[] startingItems;
    public int startingMoney = 500;

    public Currency wallet;

    private void Awake()
    {
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (!wallet) wallet = FindObjectOfType<Currency>();
    }

    private void Start()
    {
        if (wallet && startingMoney > 0) wallet.Add(startingMoney);
        if (!inventory) return;
        foreach (var e in startingItems)
            if (e.item != null && e.count > 0)
                inventory.Add(e.item, e.count);
    }
}

