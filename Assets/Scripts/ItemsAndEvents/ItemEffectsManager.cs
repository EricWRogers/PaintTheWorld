//attach to player to handle all item effects
using System.Collections.Generic;
using UnityEngine;
using SuperPupSystems.Helper;

public class ItemEffectsManager : MonoBehaviour
{
    [Header("Refs")]
    public Inventory inventory;
    public Health playerHealth;

    [Header("Enemy Targeting")]
    public LayerMask enemyLayer;

    [Header("Paint Glob")]
    public GameObject paintGlobPrefab;    // your glob prefab
    public float globSpeed = 18f;
    public int launcherGlobsPerStack = 1;
    public int giftGlobsPerStack = 1;

    [Header("Explosions")]
    public int reactiveDamage = 25;
    public float reactiveBaseRadius = 3f;
    public float reactiveRadiusPerStack = 1f;

    public int explosiveDamage = 35;
    public float explosiveBaseRadius = 3f;
    public float explosiveRadiusPerStack = 1f;

    [Header("Siphon (per hit per stack)")]
    public int siphonHealPerHit = 2;

    [Header("Fountain of Youth")]
    public GameObject healAuraPrefab;   // prefab with HealAura + SphereCollider (isTrigger)
    public float auraBaseRadius = 2f;
    public float auraRadiusPerStack = 0.8f;
    public float auraDuration = 6f;
    public int auraHealPerTick = 1;
    public float auraTickInterval = 0.3f;

    private readonly Dictionary<string,int> counts = new();
    private Transform self;

    private void Awake()
    {
        self = transform;
        if (!inventory) inventory = FindObjectOfType<Inventory>();
        if (!playerHealth) playerHealth = GetComponent<Health>();
    }

    private void OnEnable()
    {
        if (inventory) inventory.onChanged.AddListener(RefreshCounts);
        RefreshCounts();

        GameEvents.PlayerHitEnemy += OnPlayerHitEnemy;
        GameEvents.PlayerDamaged  += OnPlayerDamaged;
        GameEvents.PlayerHealed   += OnPlayerHealed;
        GameEvents.EnemyKilled    += OnEnemyKilled;
    }

    private void OnDisable()
    {
        if (inventory) inventory.onChanged.RemoveListener(RefreshCounts);
        GameEvents.PlayerHitEnemy -= OnPlayerHitEnemy;
        GameEvents.PlayerDamaged  -= OnPlayerDamaged;
        GameEvents.PlayerHealed   -= OnPlayerHealed;
        GameEvents.EnemyKilled    -= OnEnemyKilled;
    }

    private void RefreshCounts()
    {
        counts.Clear();
        if (inventory == null) return;
        foreach (var s in inventory.items)
        {
            if (s?.item == null || string.IsNullOrEmpty(s.item.id)) continue;
            counts[s.item.id] = (counts.TryGetValue(s.item.id, out var c) ? c : 0) + s.count;
        }
    }
    private int C(string id) => counts.TryGetValue(id, out var c) ? c : 0;

    // === Events, Item behavior ===

    private void OnPlayerHitEnemy(GameObject enemy, int damage, HitSource source)
    {
        // Siphoning Paint: heal on hit
        int siphon = C(ItemIds.SiphoningPaint);
        if (siphon > 0 && playerHealth && playerHealth.currentHealth > 0)
            playerHealth.Heal(siphonHealPerHit * siphon);

        // Paint Launcher: spawn globs (but don't chain from launcher globs)
        int stacks = C(ItemIds.PaintLauncher);
        if (stacks > 0 && source != HitSource.PaintLauncherGlob)
            LaunchGlobsAtClosestEnemies(stacks * launcherGlobsPerStack, HitSource.PaintLauncherGlob);
    }

    private void OnPlayerDamaged(int amount)
    {
        // Reactive Armor: explosion at player, radius grows with stacks
        int s = C(ItemIds.ReactiveArmor);
        if (s > 0)
        {
            float r = reactiveBaseRadius + reactiveRadiusPerStack * (s - 1);
            PaintExplosion.DoDamageCircle(self.position, r, reactiveDamage, enemyLayer);
        }
    }

    private void OnPlayerHealed(int amount)
    {
        // Gift of Life: globs on heal (count as "normal" hits → can trigger Paint Launcher)
        int s = C(ItemIds.GiftOfLife);
        if (s > 0)
            LaunchGlobsAtClosestEnemies(s * giftGlobsPerStack, HitSource.GiftOfLifeGlob);
    }

    private void OnEnemyKilled(GameObject enemy)
    {
        // Explosive Paint: explode where the enemy died
        int e = C(ItemIds.ExplosivePaint);
        if (e > 0)
        {
            float r = explosiveBaseRadius + explosiveRadiusPerStack * (e - 1);
            PaintExplosion.DoDamageCircle(enemy.transform.position, r, explosiveDamage, enemyLayer);
        }

        // Fountain of Youth: healing aura at death
        int f = C(ItemIds.FountainOfYouth);
        if (f > 0 && healAuraPrefab)
        {
            float r = auraBaseRadius + auraRadiusPerStack * (f - 1);
            var go = Object.Instantiate(healAuraPrefab, enemy.transform.position, Quaternion.identity);
            var aura = go.GetComponent<HealAura>();
            if (aura)
            {
                aura.radius = r;
                aura.duration = auraDuration;
                aura.healPerTick = auraHealPerTick;
                aura.tickInterval = auraTickInterval;
                aura.player = self;
                aura.playerHealth = playerHealth;
            }
        }
    }

    private void LaunchGlobsAtClosestEnemies(int count, HitSource src)
    {
        if (!paintGlobPrefab) return;
        var targets = EnemyFinder.FindClosest(self.position, count, enemyLayer);
        foreach (var t in targets)
        {
            var go = Object.Instantiate(paintGlobPrefab, self.position + Vector3.up * 1.2f, Quaternion.identity);
            var glob = go.GetComponent<PaintGlob>();
            if (!glob) glob = go.AddComponent<PaintGlob>(); // safety
            glob.Init(t.transform, globSpeed, src, enemyLayer);
        }
    }
}
