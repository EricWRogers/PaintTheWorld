using UnityEngine;

[CreateAssetMenu(fileName = "FountainOfYouth", menuName = "Items/Fountain of Youth")]
public class FountainOfYouthSO : ItemSO
{
    [Header("Aura")]
    public float baseRadius = 2f;
    public float radiusPerStack = 0.8f;
    public float duration = 6f;
    public int healPerTick = 1;
    public float tickInterval = 0.3f;

    public override void OnEnemyKilled(PlayerContext ctx, GameObject enemy, int count)
    {
        if (!ctx.healAuraPrefab || !enemy) return;

        float r = baseRadius + radiusPerStack * (count - 1);
        var go = Object.Instantiate(ctx.healAuraPrefab, enemy.transform.position, Quaternion.identity);
        var aura = go.GetComponent<HealAura>();
        if (aura)
        {
            aura.radius = r;
            aura.duration = duration;
            aura.healPerTick = healPerTick;
            aura.tickInterval = tickInterval;
            aura.player = ctx.player;
            aura.playerHealth = ctx.playerHealth;
        }
    }
}
