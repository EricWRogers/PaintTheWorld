using UnityEngine;

[CreateAssetMenu(fileName = "SiphoningPaint", menuName = "Items/Siphoning Paint")]
public class SiphoningPaintSO : ItemSO
{
    [Header("Healing")]
    public int healPerHit = 2;

    public override void OnPlayerHitEnemy(PlayerContext ctx, HitContext hit, int count)
    {
        if (ctx.playerHealth == null || ctx.playerHealth.currentHealth <= 0) return;
        int heal = Mathf.Max(1, healPerHit) * Mathf.Max(1, count);
        ctx.playerHealth.Heal(heal);
    }
}
