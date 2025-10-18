using UnityEngine;

[CreateAssetMenu(fileName = "PaintLauncher", menuName = "Items/Paint Launcher")]
public class PaintLauncherSO : ItemSO
{
    [Header("Launcher Settings")]
    public int globsPerStack = 1;

    public override void OnPlayerHitEnemy(PlayerContext ctx, HitContext hit, int count)
    {
        //Debug.Log("player hit enemy");
        // Prevent globs from creating more globs
        if (hit.source == HitSource.PaintLauncherGlob) return;

        int total = Mathf.Max(1, globsPerStack) * Mathf.Max(1, count);
        if (!ctx.paintGlobPrefab || !ctx.player) return;

        var targets = EnemyFinder.FindClosest(ctx.player.position, total, ctx.enemyLayer);
        foreach (var t in targets)
        {
            var go = Object.Instantiate(ctx.paintGlobPrefab, ctx.player.position + Vector3.up * 1.2f, Quaternion.identity);
            var glob = go.GetComponent<PaintGlob>();
            if (!glob) glob = go.AddComponent<PaintGlob>();
            glob.Init(t.transform, ctx.globSpeed, HitSource.PaintLauncherGlob, ctx.enemyLayer);
        }
    }
}
