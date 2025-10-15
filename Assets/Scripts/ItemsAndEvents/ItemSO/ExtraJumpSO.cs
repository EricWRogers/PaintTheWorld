using UnityEngine;

[CreateAssetMenu(fileName="ExtraJump", menuName="Items/Rare/Extra Jump")]
public class ExtraJumpSO : ItemSO
{
    public int jumpsPerStack = 1;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        var ej = ctx.player ? ctx.player.GetComponent<ExtraJumpController>() : null;
        if (!ej) return;
    }
    public override void OnPurchased(PlayerContext ctx, int newCount)
    {
        PlayerManager.instance.maxJumpCount += 1;
    }
    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        var ej = ctx.player ? ctx.player.GetComponent<ExtraJumpController>() : null;
        if (ej) ej.extraJumpsGranted = 0;
    }
}
