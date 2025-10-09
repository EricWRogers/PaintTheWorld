using UnityEngine;

public enum ItemRarity { Common, Rare, Epic }

public abstract class ItemSO : ScriptableObject
{
    [Header("Identity")]
    public string id;                 // ID usedd for saving and loading
    public string displayName;
    [TextArea] public string description;
    public Sprite icon;
    public ItemRarity rarity = ItemRarity.Common;

    [Header("Economy")]
    public int basePrice = 100;
    public bool stackable = true;
    public int maxStack = 99;

    public virtual int GetPriceForNext(int currentCount) => basePrice;

    // Trigger hooks 
    public virtual void OnPurchased(PlayerContext ctx, int newCount) {}
    public virtual void OnEquipped(PlayerContext ctx, int count) {}
    public virtual void OnUnequipped(PlayerContext ctx, int count) {}
    public virtual void OnPlayerHitEnemy(PlayerContext ctx, HitContext hit, int count) {}
    public virtual void OnPlayerDamaged(PlayerContext ctx, int damage, int count) {}
    public virtual void OnPlayerHealed(PlayerContext ctx, int heal, int count) {}
    public virtual void OnEnemyKilled(PlayerContext ctx, GameObject enemy, int count) {}
}

// Contexts that item hooks can use
public struct PlayerContext
{
    public Transform player;
    public SuperPupSystems.Helper.Health playerHealth;
    public LayerMask enemyLayer;
    public GameObject paintGlobPrefab;
    public GameObject healAuraPrefab;
    public float globSpeed;
}

public struct HitContext
{
    public GameObject enemy;
    public int damage;
    public HitSource source;      
    public Vector3 position;
}
