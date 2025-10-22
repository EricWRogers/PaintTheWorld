using UnityEngine;

[CreateAssetMenu(fileName="WiderPaintTrail", menuName="Items/Common/Wider Paint Trail")]
public class WiderPaintTrailSO : ItemSO
{
    public float baseMultiplier = 1.4f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) { Debug.LogWarning("[WiderPaintTrailSO] No ctx.player."); return; }

        var scaler = ctx.player.GetComponentInChildren<PlayerPaintWidthScaler>(true);
        if (!scaler) { Debug.LogWarning("[WiderPaintTrailSO] No PlayerPaintWidthScaler found under player."); return; }

        float mult = Mathf.Pow(baseMultiplier, Mathf.Max(1, count));
        scaler.widthMultiplier = mult;
        Debug.Log($"[WiderPaintTrailSO] Applied widthMultiplier={mult} on {scaler.gameObject.name}");
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;
        var scaler = ctx.player.GetComponentInChildren<PlayerPaintWidthScaler>(true);
        if (scaler) scaler.widthMultiplier = 1f;
    }
}
