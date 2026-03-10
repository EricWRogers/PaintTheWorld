using UnityEngine;

[CreateAssetMenu(menuName = "Items/Dash Paint Splash")]
public class DashPaintSplashItemSO : ItemSO
{
    [Header("Splash Settings")]
    public float baseRadius = 1.2f;
    public float radiusPerStack = 0.5f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Ground Detection")]
    public float rayStartOffset = 0.75f;
    public float rayDistance = 3f;
    public LayerMask paintMask = ~0;

    [Header("Sampling")]
    public int rings = 2;
    public int samplesPerRing = 8;
    public float minStampRadius = 0.45f;
    public float stampRadiusMultiplier = 0.35f;

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
        emitter.rings = rings;
        emitter.samplesPerRing = samplesPerRing;
        emitter.minStampRadius = minStampRadius;
        emitter.stampRadiusMultiplier = stampRadiusMultiplier;

        emitter.EmitBurst(count);
    }
}