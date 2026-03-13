using UnityEngine;

[CreateAssetMenu(fileName = "GrindStartEndSplash", menuName = "Items/Legendary/Grind Start End Splash")]
public class GrindStartEndSplashSO : ItemSO
{
    [Header("Ground Splash")]
    public float basePaintRadius = 2.0f;
    public float radiusPerStack = 0.75f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Ground Detection")]
    public float rayStartHeight = 3.0f;
    public float rayDistance = 10f;
    public LayerMask groundMask = ~0;

    [Header("Projectile Ring")]
    public int baseGlobCount = 8;
    public int globCountPerStack = 2;
    public float spawnHeight = 0.35f;
    public float globRingRadius = 0.6f;
    public float outwardForceMultiplier = 1f;

    public override void OnGrindStart(PlayerContext ctx, int count)
    {
        EmitSplash(ctx, count);
    }

    public override void OnGrindEnd(PlayerContext ctx, int count)
    {
        EmitSplash(ctx, count);
    }

    void EmitSplash(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<GrindSplashEmitter>();
        if (!emitter)
            emitter = ctx.player.gameObject.AddComponent<GrindSplashEmitter>();

        emitter.basePaintRadius = basePaintRadius;
        emitter.radiusPerStack = radiusPerStack;
        emitter.hardness = hardness;
        emitter.strength = strength;
        emitter.rayStartHeight = rayStartHeight;
        emitter.rayDistance = rayDistance;
        emitter.groundMask = groundMask;
        emitter.baseGlobCount = baseGlobCount;
        emitter.globCountPerStack = globCountPerStack;
        emitter.spawnHeight = spawnHeight;
        emitter.globRingRadius = globRingRadius;
        emitter.outwardForceMultiplier = outwardForceMultiplier;

        emitter.Emit(count);
    }
}