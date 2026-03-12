using UnityEngine;

[CreateAssetMenu(fileName = "SmearOnPaintStep", menuName = "Items/Common/Smear On Paint Step")]
public class SmearOnPaintStepSO : ItemSO
{
    [Header("Smear Settings")]
    public float smearDuration = 1.0f;
    public float baseRadius = 0.9f;
    public float radiusPerStack = 0.3f;
    public float hardness = 1f;
    public float strength = 1f;
    public float tickInterval = 0.05f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<PaintSmearEmitter>();
        if (!emitter)
            emitter = ctx.player.gameObject.AddComponent<PaintSmearEmitter>();

        emitter.Configure(count, smearDuration, baseRadius, radiusPerStack);
        emitter.hardness = hardness;
        emitter.strength = strength;
        emitter.tickInterval = tickInterval;
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<PaintSmearEmitter>();
        if (emitter)
            Object.Destroy(emitter);
    }
}