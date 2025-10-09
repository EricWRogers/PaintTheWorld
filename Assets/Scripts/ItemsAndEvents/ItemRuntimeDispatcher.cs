using UnityEngine;

public class ItemRuntimeDispatcher : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.PlayerHitEnemy += OnHit;
        GameEvents.PlayerDamaged  += OnDamaged;
        GameEvents.PlayerHealed   += OnHealed;
        GameEvents.EnemyKilled    += OnKilled;
    }
    void OnDisable()
    {
        GameEvents.PlayerHitEnemy -= OnHit;
        GameEvents.PlayerDamaged  -= OnDamaged;
        GameEvents.PlayerHealed   -= OnHealed;
        GameEvents.EnemyKilled    -= OnKilled;
    }

    PlayerContext Ctx => PlayerManager.instance.GetContext();

    void ForEachItem(System.Action<ItemSO,int> action)
    {
        var inv = PlayerManager.instance.inventory.items;
        for (int i = 0; i < inv.Count; i++)
        {
            var s = inv[i];
            if (s?.item == null || s.count <= 0) continue;
            action(s.item, s.count);
        }
    }

    void OnHit(GameObject enemy, int damage, HitSource src)
    {
        var hc = new HitContext { enemy = enemy, damage = damage, source = src, position = enemy ? enemy.transform.position : Vector3.zero };
        ForEachItem((it,c)=> it.OnPlayerHitEnemy(Ctx, hc, c));
    }
    void OnDamaged(int dmg)          => ForEachItem((it,c)=> it.OnPlayerDamaged(Ctx, dmg, c));
    void OnHealed(int heal)          => ForEachItem((it,c)=> it.OnPlayerHealed(Ctx, heal, c));
    void OnKilled(GameObject enemy)  => ForEachItem((it,c)=> it.OnEnemyKilled(Ctx, enemy, c));
}

