using UnityEngine;

[CreateAssetMenu(fileName = "BonusAttackSpeed", menuName = "Items/Common/Bonus Attack Speed")]
public class BonusAttackSpeedSO : ItemSO
{
    [Header("Attack Speed Bonus")]
    public float attackSpeedMultiplierPerStack = 0.2f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var spray = ctx.player.GetComponentInChildren<SprayPaintLine>(true);
        if (!spray) return;

        int stacks = Mathf.Max(1, count);

       
        float multiplier = 1f + attackSpeedMultiplierPerStack * stacks;
        spray.SetAttackSpeedMultiplier(multiplier);
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var spray = ctx.player.GetComponentInChildren<SprayPaintLine>(true);
        if (!spray) return;

        spray.SetAttackSpeedMultiplier(1f);
    }
}