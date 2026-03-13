using UnityEngine;

[CreateAssetMenu(menuName = "Items/Dash Paint Splash")]
public class DashPaintSplashItemSO : ItemSO
{
    [Header("Trail Settings")]
    public float baseRadius = 0.9f;
    public float radiusPerStack = 0.3f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Timing")]
    public float emitDuration = 0.5f; // match dash duration

    [Header("Ground Detection")]
    public float rayStartOffset = 0.6f;
    public float rayDistance = 2.5f;
    public LayerMask paintMask = ~0;

    [Header("Sampling")]
    public float tickInterval = 0.05f;

    public override void OnDodged(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<DashSplashEmitter>();
        if (!emitter)
            emitter = ctx.player.gameObject.AddComponent<DashSplashEmitter>();

        emitter.baseRadius = baseRadius;
        emitter.radiusPerStack = radiusPerStack;
        emitter.hardness = hardness;
        emitter.strength = strength;
        emitter.rayStartOffset = rayStartOffset;
        emitter.rayDistance = rayDistance;
        emitter.paintMask = paintMask;
        emitter.tickInterval = tickInterval;

        emitter.EmitForDuration(count, emitDuration);
    }
}