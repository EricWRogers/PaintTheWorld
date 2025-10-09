using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/Item Database")]
public class ItemDatabase : ScriptableObject
{
    public List<ItemSO> items = new();
    private Dictionary<string, ItemSO> byId;

    public void BuildIndex()
    {
        byId = new Dictionary<string, ItemSO>();
        foreach (var it in items)
        {
            if (!it || string.IsNullOrEmpty(it.id)) continue;
            byId[it.id] = it;
        }
    }

    public ItemSO Get(string id)
    {
        if (byId == null) BuildIndex();
        return (!string.IsNullOrEmpty(id) && byId.TryGetValue(id, out var it)) ? it : null;
    }

    private static ItemDatabase _instance;
    public static ItemDatabase Load()
    {
        if (_instance) return _instance;
        _instance = Resources.Load<ItemDatabase>("ItemDatabase/ItemDatabase");
        if (_instance) _instance.BuildIndex();
        return _instance;
    }
}
