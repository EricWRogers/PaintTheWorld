using System.Collections.Generic;
using UnityEngine;

public class ItemRuntimeDispatcher : MonoBehaviour
{
    private readonly Dictionary<string, int> appliedCounts = new();

    void OnEnable()
    {
        GameEvents.PlayerHitEnemy += OnHit;
        GameEvents.PlayerDamaged += OnDamaged;
        GameEvents.PlayerHealed += OnHealed;
        GameEvents.EnemyKilled += OnKilled;
        GameEvents.PlayerDodged += OnDodged;
        GameEvents.PlayerStartedGrinding += OnGrindStart;
        GameEvents.PlayerGrindingTick += OnGrindTick;
        GameEvents.PlayerEndedGrinding += OnGrindEnd;
        GameEvents.PaintApplied += OnPaintApplied;
        GameEvents.PlayerLanded += OnLanded;
        GameEvents.EnemyRecoveredFromStun += OnEnemyRecoveredFromStun;

        RefreshEquippedItems();
    }

    void OnDisable()
    {
        GameEvents.PlayerHitEnemy -= OnHit;
        GameEvents.PlayerDamaged -= OnDamaged;
        GameEvents.PlayerHealed -= OnHealed;
        GameEvents.EnemyKilled -= OnKilled;
        GameEvents.PlayerDodged -= OnDodged;
        GameEvents.PlayerStartedGrinding -= OnGrindStart;
        GameEvents.PlayerGrindingTick -= OnGrindTick;
        GameEvents.PlayerEndedGrinding -= OnGrindEnd;
        GameEvents.PaintApplied -= OnPaintApplied;
        GameEvents.PlayerLanded -= OnLanded;
        GameEvents.EnemyRecoveredFromStun -= OnEnemyRecoveredFromStun;
    }

    void Update()
    {
        RefreshEquippedItems();
    }

    PlayerContext Ctx => PlayerManager.instance.GetContext();

    void ForEachItem(System.Action<ItemSO, int> action)
    {
        var inv = PlayerManager.instance.inventory.items;
        for (int i = 0; i < inv.Count; i++)
        {
            var s = inv[i];
            if (s?.item == null || s.count <= 0) continue;
            action(s.item, s.count);
        }
    }

    void RefreshEquippedItems()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.inventory == null)
            return;

        Dictionary<string, int> currentCounts = new();

        var inv = PlayerManager.instance.inventory.items;
        for (int i = 0; i < inv.Count; i++)
        {
            var s = inv[i];
            if (s?.item == null || s.count <= 0) continue;

            currentCounts[s.item.id] = s.count;

            if (!appliedCounts.TryGetValue(s.item.id, out int oldCount) || oldCount != s.count)
            {
                s.item.OnEquipped(Ctx, s.count);
                appliedCounts[s.item.id] = s.count;
            }
        }

        List<string> removedIds = null;

        foreach (var kvp in appliedCounts)
        {
            if (!currentCounts.ContainsKey(kvp.Key))
            {
                if (removedIds == null)
                    removedIds = new List<string>();

                removedIds.Add(kvp.Key);
            }
        }

        if (removedIds != null)
        {
            ItemDatabase db = ItemDatabase.Load();

            foreach (string id in removedIds)
            {
                ItemSO item = db != null ? db.Get(id) : null;
                if (item != null)
                {
                    item.OnUnequipped(Ctx, 0);
                }

                appliedCounts.Remove(id);
            }
        }
    }

    void OnHit(GameObject enemy, int damage, HitSource src)
    {
        var hc = new HitContext
        {
            enemy = enemy,
            damage = damage,
            source = src,
            position = enemy ? enemy.transform.position : Vector3.zero
        };

        ForEachItem((it, c) => it.OnPlayerHitEnemy(Ctx, hc, c));
    }

    void OnLanded() => ForEachItem((it, c) => it.OnLanded(Ctx, c));
    void OnDamaged(int dmg) => ForEachItem((it, c) => it.OnPlayerDamaged(Ctx, dmg, c));
    void OnHealed(int heal) => ForEachItem((it, c) => it.OnPlayerHealed(Ctx, heal, c));
    void OnKilled(GameObject enemy) => ForEachItem((it, c) => it.OnEnemyKilled(Ctx, enemy, c));
    void OnDodged() => ForEachItem((it, c) => it.OnDodged(Ctx, c));
    void OnGrindStart() => ForEachItem((it, c) => it.OnGrindStart(Ctx, c));
    void OnGrindTick() => ForEachItem((it, c) => it.OnGrindTick(Ctx, c));
    void OnGrindEnd() => ForEachItem((it, c) => it.OnGrindEnd(Ctx, c));
    void OnPaintApplied(float amt) => ForEachItem((it, c) => it.OnPaintApplied(Ctx, amt, c));
    void OnEnemyRecoveredFromStun(GameObject enemy) => ForEachItem((it, c) => it.OnEnemyRecoveredFromStun(Ctx, enemy, c));
}