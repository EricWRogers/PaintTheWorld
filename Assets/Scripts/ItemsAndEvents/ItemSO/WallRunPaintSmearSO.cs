using UnityEngine;

[CreateAssetMenu(fileName = "WallRunPaintSmear", menuName = "Items/Rare/Wall Run Paint Smear")]
public class WallRunPaintSmearSO : ItemSO
{
    [Header("Smear")]
    public float tickInterval = 0.05f;
    public float baseRadius = 0.7f;
    public float radiusPerStack = 0.2f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Wall Sampling")]
    public float wallRayDistance = 1.5f;
    public float verticalOffset = 1.0f;
    public float wallSurfaceOffset = 0.05f;

    [Header("Drips")]
    public int baseDripCount = 2;
    public int dripCountPerStack = 1;
    public float dripDuration = 0.75f;
    public float dripSpeed = 1.8f;
    public float dripSpacing = 0.3f;
    public float dripRadiusMultiplier = 0.7f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<WallRunPaintSmearEmitter>();
        if (!emitter)
            emitter = ctx.player.gameObject.AddComponent<WallRunPaintSmearEmitter>();

        emitter.tickInterval = tickInterval;
        emitter.baseRadius = baseRadius;
        emitter.radiusPerStack = radiusPerStack;
        emitter.hardness = hardness;
        emitter.strength = strength;
        emitter.wallRayDistance = wallRayDistance;
        emitter.verticalOffset = verticalOffset;
        emitter.wallSurfaceOffset = wallSurfaceOffset;
        emitter.baseDripCount = baseDripCount;
        emitter.dripCountPerStack = dripCountPerStack;
        emitter.dripDuration = dripDuration;
        emitter.dripSpeed = dripSpeed;
        emitter.dripSpacing = dripSpacing;
        emitter.dripRadiusMultiplier = dripRadiusMultiplier;

        emitter.Configure(count);
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<WallRunPaintSmearEmitter>();
        if (emitter)
            Object.Destroy(emitter);
    }
}