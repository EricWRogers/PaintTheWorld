using UnityEngine;

[CreateAssetMenu(fileName="WiderPaintTrail", menuName="Items/Common/Wider Paint Trail")]
public class WiderPaintTrailSO : ItemSO
{
    public float baseMultiplier = 4f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        var scaler = ctx.player ? ctx.player.GetComponent<PlayerPaintWidthScaler>() : null;
        if (!scaler) return;
        scaler.widthMultiplier = Mathf.Pow(baseMultiplier, Mathf.Max(1, count));
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        var scaler = ctx.player ? ctx.player.GetComponent<PlayerPaintWidthScaler>() : null;
        if (scaler) scaler.widthMultiplier = 1f;
    }
}

