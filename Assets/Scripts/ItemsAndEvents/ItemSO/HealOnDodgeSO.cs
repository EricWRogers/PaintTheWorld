using UnityEngine;

[CreateAssetMenu(fileName="HealOnDodge", menuName="Items/Common/Heal on Dodge")]
public class HealOnDodgeSO : ItemSO
{
    public int healPerStack = 10;

    public override void OnDodged(PlayerContext ctx, int count)
    {
        if (ctx.playerHealth == null || ctx.playerHealth.currentHealth <= 0) return;
        ctx.playerHealth.Heal(Mathf.Max(1, healPerStack) * Mathf.Max(1, count));
    }
}
