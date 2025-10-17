using UnityEngine;

public class DebugGiveItems : MonoBehaviour
{
    public ItemSO[] items;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
            foreach (var it in items) PlayerManager.instance.inventory.Add(it, 1);
        if (Input.GetKeyDown(KeyCode.F2))
            GameEvents.PlayerDamaged?.Invoke(10);          // test Reactive Armor
        if (Input.GetKeyDown(KeyCode.F3))
            GameEvents.PlayerHealed?.Invoke(10);           // test Gift of Life
    }
}

