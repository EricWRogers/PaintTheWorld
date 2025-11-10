using UnityEngine;

[CreateAssetMenu(fileName="PaintTrail", menuName="Items/Common/Paint Trail")]
public class PaintTrailSO : ItemSO
{
    [Header("Trail Tuning")]
    public float baseRadius = 0.6f;
    public float radiusPerStack = 0.2f;
    public float hardness = 1f;
    public float strength = 1f;
    public float tickInterval = 0.06f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;

        var emitter = ctx.player.GetComponent<PaintTrailEmitter>();
        if (!emitter) emitter = ctx.player.gameObject.AddComponent<PaintTrailEmitter>();

    
        emitter.baseRadius = baseRadius;
        emitter.radiusPerStack = radiusPerStack;
        emitter.hardness = hardness;
        emitter.strength = strength;
        emitter.tickInterval = tickInterval;

        // Recalculate stacks from inventory 
        int stacks = PlayerManager.instance ? PlayerManager.instance.inventory.GetCount(id) : Mathf.Max(1, count);
        emitter.Configure(stacks);
        emitter.enabled = true;

        Debug.Log($"[Item] PaintTrail equipped, stacks={stacks}");
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;
        var emitter = ctx.player.GetComponent<PaintTrailEmitter>();
        if (!emitter) return;

        // Remaining stacks after unequip
        int stacks = PlayerManager.instance ? PlayerManager.instance.inventory.GetCount(id) : 0;
        if (stacks <= 0)
        {
            Object.Destroy(emitter);
        }
        else
        {
            emitter.Configure(stacks);
        }
        Debug.Log($"[Item] PaintTrail unequipped, remaining stacks={stacks}");
    }
}

