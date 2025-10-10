using UnityEngine;

public class ShopCatalog : MonoBehaviour
{
    [Header("Items sold here (ItemSO)")]
    public ItemSO[] items;

    [Header("Runtime stock (-1 = unlimited)")]
    public int[] stockRuntime;

    private void OnEnable()
    {
        if (items == null) items = new ItemSO[0];

        if (stockRuntime == null || stockRuntime.Length != items.Length)
        {
            stockRuntime = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
                stockRuntime[i] = -1; // default unlimited; add per-item if you like
        }
    }

    private void OnValidate()
    {
        if (items == null) return;
        if (stockRuntime == null || stockRuntime.Length != items.Length)
        {
            var newStock = new int[items.Length];
            for (int i = 0; i < items.Length; i++)
                newStock[i] = (i < (stockRuntime?.Length ?? 0)) ? stockRuntime[i] : -1;
            stockRuntime = newStock;
        }
    }

    public bool HasStock(int index) => (index >= 0 && index < stockRuntime.Length) && stockRuntime[index] != 0;
    public void ConsumeStock(int index) { if (index >= 0 && index < stockRuntime.Length && stockRuntime[index] > 0) stockRuntime[index]--; }
    public int GetStock(int index) => (index >= 0 && index < stockRuntime.Length) ? stockRuntime[index] : 0;
}
