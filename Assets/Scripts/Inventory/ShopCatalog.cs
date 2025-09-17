using UnityEngine;

public class ShopCatalog : MonoBehaviour
{
    [Header("Items sold here")]
    public ItemDefinition[] items;

    [Header("Runtime stock (mirrors 'initialStock')")]
    public int[] stockRuntime; // -1 = unlimited

    private void OnEnable()
    {
        if (items == null) items = new ItemDefinition[0];

        if (stockRuntime == null || stockRuntime.Length != items.Length)
        {
            stockRuntime = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                stockRuntime[i] = (items[i] != null) ? items[i].initialStock : 0; 
            }
        }
    }

    
    private void OnValidate()
    {
        if (items == null) return;
        if (stockRuntime == null || stockRuntime.Length != items.Length)
        {
            var newStock = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                newStock[i] = (i < (stockRuntime?.Length ?? 0)) 
                    ? stockRuntime[i] 
                    : (items[i] != null ? items[i].initialStock : 0);
            }
            stockRuntime = newStock;
        }
    }

    public bool HasStock(int index)
    {
        if (index < 0 || index >= stockRuntime.Length) return false;
        // -1 means unlimited, >0 means remaining, 0 means out
        return stockRuntime[index] != 0;
    }

    public void ConsumeStock(int index)
    {
        if (index < 0 || index >= stockRuntime.Length) return;
        if (stockRuntime[index] > 0) stockRuntime[index]--;
        // if -1, unlimited — do nothing
    }

    // optional helper
    public int GetStock(int index) => (index >= 0 && index < stockRuntime.Length) ? stockRuntime[index] : 0;
}
