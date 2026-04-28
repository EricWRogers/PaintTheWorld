using UnityEngine;

[CreateAssetMenu(fileName = "ExtraAirDashItem", menuName = "Items/Extra Air Dash Item")]
public class ExtraAirDashItemSO : ItemSO
{
    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (ctx.player == null) return;

        PlayerMovement movement = ctx.player.GetComponent<PlayerMovement>();
        if (movement == null) return;

        movement.SetBonusAirDashes(count);
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (ctx.player == null) return;

        PlayerMovement movement = ctx.player.GetComponent<PlayerMovement>();
        if (movement == null) return;

        movement.SetBonusAirDashes(0);
    }
}