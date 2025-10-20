using UnityEngine;

[CreateAssetMenu(fileName="GrindBurst", menuName="Items/Legendary/Grind Burst")]
public class GrindBurstSO : ItemSO
{
    public int spokesPerStack = 6;   // shots per ring
    public float spawnHeight = 0.1f;
    public float ringSpeed = 18f;

    public override void OnGrindStart(PlayerContext ctx, int count) => FireRing(ctx, count);
    public override void OnGrindTick(PlayerContext ctx, int count)  => FireRing(ctx, count);

    private void FireRing(PlayerContext ctx, int count)
    {
        if (!ctx.paintGlobPrefab || !ctx.player) return;
        int n = Mathf.Max(1, spokesPerStack) * Mathf.Max(1, count);
        Vector3 origin = ctx.player.position + Vector3.up * spawnHeight;

        for (int i = 0; i < n; i++)
        {
            float ang = (360f / n) * i;
            Vector3 dir = Quaternion.Euler(0, ang, 0) * Vector3.forward;
            var go = Object.Instantiate(ctx.paintGlobPrefab, origin, Quaternion.LookRotation(dir));
            var glob = go.GetComponent<PaintGlob>() ?? go.AddComponent<PaintGlob>();
            glob.Init(null, ringSpeed, HitSource.PaintLauncherGlob, ctx.enemyLayer); //globs fly straight if no target
        }
    }
}

