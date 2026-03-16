using UnityEngine;

[CreateAssetMenu(fileName = "BonusProjectileCount", menuName = "Items/Common/Bonus Projectile Count")]
public class BonusProjectileCountSO : ItemSO
{
    [Header("Projectile Bonus")]
    public int projectilesPerStack = 1;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var spray = ctx.player.GetComponentInChildren<SprayPaintLine>(true);
        if (!spray) return;

        int stacks = Mathf.Max(1, count);
        spray.SetBonusProjectileCount(projectilesPerStack * stacks);
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var spray = ctx.player.GetComponentInChildren<SprayPaintLine>(true);
        if (!spray) return;

        spray.SetBonusProjectileCount(0);
    }
}