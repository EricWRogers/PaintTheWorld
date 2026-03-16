using UnityEngine;

[CreateAssetMenu(fileName = "BonusMoveSpeed", menuName = "Items/Common/Bonus Move Speed")]
public class BonusMoveSpeedSO : ItemSO
{
    [Header("Move Speed Bonus")]
    public float moveSpeedPerStack = 1f;
    public float maxSpeedPerStack = 2f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var movement = ctx.player.GetComponent<PlayerMovement>();
        if (!movement) return;

        int stacks = Mathf.Max(1, count);

        movement.SetMoveSpeedBonus(
            moveSpeedPerStack * stacks,
            maxSpeedPerStack * stacks
        );
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var movement = ctx.player.GetComponent<PlayerMovement>();
        if (!movement) return;

        movement.ClearMoveSpeedBonus();
    }
}