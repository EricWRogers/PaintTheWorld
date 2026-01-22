using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class ShopTableManager : MonoBehaviour
{
    [Header("Database")]
    public ItemDatabase database;      

    [Header("Layout")]
    public Transform[] spawnPoints;    
    public ShopItemStand standPrefab;

    [Header("Reroll")]
    public KeyCode rerollKey = KeyCode.R;
    public bool allowOneReroll = true;
    public float priceMultiplierAfterReroll = 1.25f;

    private readonly List<ShopItemStand> _stands = new();
    private bool _rerolled = false;

    void Start()
    {
        if (!database)
            database = ItemDatabase.Load(); // fallback to Resources/ItemDatabase/ItemDatabase

        BuildInitialSelection();
    }

    void Update()
    {
        if (allowOneReroll && !_rerolled && Input.GetKeyDown(rerollKey))
            Reroll();
    }

    void BuildInitialSelection()
    {
        ClearStands();

        var pool = database.items.Where(i => i).ToList();
        var chosen = ChooseRandomDistinct(pool, Mathf.Min(3, spawnPoints.Length));

        for (int i = 0; i < chosen.Count; i++)
        {
            var stand = Instantiate(standPrefab, spawnPoints[i].position, spawnPoints[i].rotation, transform);
            stand.Setup(chosen[i], -1);
            _stands.Add(stand);
        }
    }

    void Reroll()
    {
        _rerolled = true;

        // You can allow alreadysold slots to stay empty, or restock all non-sold stands:
        var pool = database.items.Where(i => i).ToList();
        var chosen = ChooseRandomDistinct(pool, _stands.Count);

        for (int i = 0; i < _stands.Count; i++)
        {
            var stand = _stands[i];
            if (!stand || stand.IsSold) continue; // don’t touch sold slots

            stand.Setup(chosen[i], -1);
            stand.SetPriceMultiplier(priceMultiplierAfterReroll);
            stand.RefreshUI();
        }
    }

    void ClearStands()
    {
        foreach (var s in _stands)
            if (s) Destroy(s.gameObject);
        _stands.Clear();
    }

    static List<ItemSO> ChooseRandomDistinct(List<ItemSO> pool, int count)
    {
        var list = new List<ItemSO>(pool);
        var result = new List<ItemSO>();
        for (int i = 0; i < count && list.Count > 0; i++)
        {
            int k = Random.Range(0, list.Count);
            result.Add(list[k]);
            list.RemoveAt(k);
        }
        return result;
    }
}
