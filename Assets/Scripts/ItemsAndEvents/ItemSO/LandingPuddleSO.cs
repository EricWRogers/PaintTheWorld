using UnityEngine;

[CreateAssetMenu(fileName = "LandingPuddle", menuName = "Items/Common/Landing Puddle")]
public class LandingPuddleSO : ItemSO
{
    [Header("Puddle Settings")]
    public float baseRadius = 2.0f;
    public float radiusPerStack = 0.75f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Ground Detection")]
    public float rayStartOffset = 0.75f;
    public float rayDistance = 3f;
    public LayerMask paintMask = ~0;

    [Header("Shape")]
    public int rings = 3;
    public int samplesPerRing = 10;
    public float minStampRadius = 0.5f;
    public float stampRadiusMultiplier = 0.35f;

    public override void OnLanded(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<LandingSplashEmitter>();
        if (!emitter)
            emitter = ctx.player.gameObject.AddComponent<LandingSplashEmitter>();

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

        emitter.EmitPuddle(count);
    }
}