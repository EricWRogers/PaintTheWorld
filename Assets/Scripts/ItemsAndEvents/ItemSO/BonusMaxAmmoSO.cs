using UnityEngine;

[CreateAssetMenu(fileName = "BonusMaxAmmo", menuName = "Items/Common/Bonus Max Ammo")]
public class BonusMaxAmmoSO : ItemSO
{
    [Header("Ammo Bonus")]
    public int ammoPerStack = 50;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var spray = ctx.player.GetComponentInChildren<SprayPaintLine>(true);
        if (!spray) return;

        int stacks = Mathf.Max(1, count);
        spray.SetBonusMaxAmmo(ammoPerStack * stacks, true);
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var spray = ctx.player.GetComponentInChildren<SprayPaintLine>(true);
        if (!spray) return;

        spray.SetBonusMaxAmmo(0, false);
    }
}