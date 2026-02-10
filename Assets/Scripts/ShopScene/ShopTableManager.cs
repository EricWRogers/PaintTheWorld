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
    private float _currentPriceMultiplier = 1f;

    void Start()
    {
        if (!database) database = ItemDatabase.Load();
        BuildOrReuseStands();
        FillShopWithRandomItems();
    }

    void Update()
    {
        if (allowOneReroll && !_rerolled && Input.GetKeyDown(rerollKey))
            Reroll();
    }

    void BuildOrReuseStands()
    {
        if (_stands.Count > 0) return;
        if (!standPrefab || spawnPoints == null || spawnPoints.Length == 0) return;

        int count = Mathf.Min(3, spawnPoints.Length);
        for (int i = 0; i < count; i++)
        {
            var stand = Instantiate(standPrefab, spawnPoints[i].position, spawnPoints[i].rotation, transform);
            _stands.Add(stand);
        }
    }

    void FillShopWithRandomItems()
    {
        if (database == null || database.items == null || database.items.Count == 0) return;

        var pm = PlayerManager.instance;
        var inv = pm ? pm.inventory : null;

        
        var poolPreferred = database.items
            .Where(i => i != null && !string.IsNullOrEmpty(i.id))
            .Where(i => inv == null || inv.GetCount(i.id) == 0)
            .ToList();

        
        var poolAll = database.items
            .Where(i => i != null && !string.IsNullOrEmpty(i.id))
            .ToList();

        var pool = (poolPreferred.Count >= _stands.Count) ? poolPreferred : poolAll;

        // Pick items for each stand
        var chosen = ChooseForStands(pool, _stands.Count);

        for (int i = 0; i < _stands.Count; i++)
        {
            var stand = _stands[i];
            if (!stand) continue;

            stand.SetPriceMultiplier(_currentPriceMultiplier); 
            stand.Setup(chosen[i], -1);
            stand.RefreshUI();
        }
    }

    void Reroll()
    {
        _rerolled = true;
        _currentPriceMultiplier *= priceMultiplierAfterReroll; // prices go up
        FillShopWithRandomItems(); // restocks all stands
    }

   
    static List<ItemSO> ChooseForStands(List<ItemSO> pool, int count)
    {
        var result = new List<ItemSO>(count);

        if (pool == null || pool.Count == 0)
        {
            for (int i = 0; i < count; i++) result.Add(null);
            return result;
        }

        if (pool.Count >= count)
        {
            var temp = new List<ItemSO>(pool);
            for (int i = 0; i < count; i++)
            {
                int k = Random.Range(0, temp.Count);
                result.Add(temp[k]);
                temp.RemoveAt(k);
            }
            return result;
        }

       
        for (int i = 0; i < count; i++)
        {
            ItemSO pick = pool[Random.Range(0, pool.Count)];

           
            for (int tries = 0; tries < 4 && i > 0 && pick == result[i - 1] && pool.Count > 1; tries++)
                pick = pool[Random.Range(0, pool.Count)];

            result.Add(pick);
        }
        return result;
    }
}
