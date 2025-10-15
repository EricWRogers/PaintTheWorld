using UnityEngine;

[CreateAssetMenu(fileName="GrindBurst", menuName="Items/Legendary/Grind Burst")]
public class GrindBurstSO : ItemSO
{
    public int globsOnStartPerStack = 3;
    public int globsPerTickPerStack = 1;

    public override void OnGrindStart(PlayerContext ctx, int count)
    {
        Launch(ctx, globsOnStartPerStack * Mathf.Max(1, count));
    }

    public override void OnGrindTick(PlayerContext ctx, int count)
    {
        Launch(ctx, globsPerTickPerStack * Mathf.Max(1, count));
    }

    private void Launch(PlayerContext ctx, int n)
    {
        if (!ctx.paintGlobPrefab || !ctx.player || n <= 0) return;
        var targets = EnemyFinder.FindClosest(ctx.player.position, n, ctx.enemyLayer);
        foreach (var t in targets)
        {
            var go = Object.Instantiate(ctx.paintGlobPrefab, ctx.player.position + Vector3.up * 1.2f, Quaternion.identity);
            var glob = go.GetComponent<PaintGlob>() ?? go.AddComponent<PaintGlob>();
            glob.Init(t.transform, ctx.globSpeed, HitSource.PaintLauncherGlob, ctx.enemyLayer);
        }
    }
}

