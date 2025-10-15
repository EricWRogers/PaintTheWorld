using UnityEngine;

[CreateAssetMenu(fileName="ExtraJump", menuName="Items/Rare/Extra Jump")]
public class ExtraJumpSO : ItemSO
{
    public int jumpsPerStack = 1;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        var ej = ctx.player ? ctx.player.GetComponent<ExtraJumpController>() : null;
        if (!ej) return;
        ej.extraJumpsGranted = Mathf.Max(0, jumpsPerStack * Mathf.Max(1, count));
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        var ej = ctx.player ? ctx.player.GetComponent<ExtraJumpController>() : null;
        if (ej) ej.extraJumpsGranted = 0;
    }
}
