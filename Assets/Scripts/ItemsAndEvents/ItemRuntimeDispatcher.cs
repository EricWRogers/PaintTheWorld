using UnityEngine;

public class ItemRuntimeDispatcher : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.PlayerHitEnemy += OnHit;
        GameEvents.PlayerDamaged  += OnDamaged;
        GameEvents.PlayerHealed   += OnHealed;
        GameEvents.EnemyKilled    += OnKilled;
        GameEvents.PlayerDodged         += OnDodged;
        GameEvents.PlayerStartedGrinding+= OnGrindStart;
        GameEvents.PlayerGrindingTick   += OnGrindTick;
        GameEvents.PaintApplied += OnPaintApplied;
        GameEvents.WallRunStarted += OnGrindStart;
        GameEvents.WallRunTick    += OnGrindTick;
    }
    void OnDisable()
    {
        GameEvents.PlayerHitEnemy -= OnHit;
        GameEvents.PlayerDamaged  -= OnDamaged;
        GameEvents.PlayerHealed   -= OnHealed;
        GameEvents.EnemyKilled    -= OnKilled;
        GameEvents.PlayerDodged          -= OnDodged;
        GameEvents.PlayerStartedGrinding -= OnGrindStart;
        GameEvents.PlayerGrindingTick    -= OnGrindTick;
        GameEvents.PaintApplied -= OnPaintApplied;
        GameEvents.WallRunStarted -= OnGrindStart;
        GameEvents.WallRunTick    -= OnGrindTick;
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
    void OnKilled(GameObject enemy) => ForEachItem((it, c) => it.OnEnemyKilled(Ctx, enemy, c));
    void OnDodged()                 => ForEachItem((it,c)=> it.OnDodged(Ctx, c));
    void OnGrindStart()             => ForEachItem((it,c)=> it.OnGrindStart(Ctx, c));
    void OnGrindTick()              => ForEachItem((it,c)=> it.OnGrindTick(Ctx, c));
    void OnPaintApplied(float amt) => ForEachItem((it, c) => it.OnPaintApplied(Ctx, amt, c));
    

}

