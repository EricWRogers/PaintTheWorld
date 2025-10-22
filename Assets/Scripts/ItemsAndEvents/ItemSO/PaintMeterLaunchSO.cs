using UnityEngine;

[CreateAssetMenu(fileName="PaintMeterLauncher", menuName="Items/Legendary/Paint Meter Launcher")]
public class PaintMeterLauncherSO : ItemSO
{
    public float baseThreshold = 110f; // lower as needed
    public int globs = 6;
    private float progress;

    public override void OnPaintApplied(PlayerContext ctx, float amount, int count)
    {
        progress += Mathf.Max(0, amount);
        float threshold = Mathf.Max(1f, baseThreshold) / Mathf.Max(1, count);
        if (progress >= threshold)
        {
            progress = 0f;
            if (!ctx.paintGlobPrefab || !ctx.player) return;
            var targets = EnemyFinder.FindClosest(ctx.player.position, globs, ctx.enemyLayer);
            foreach (var t in targets)
            {
                var go = Object.Instantiate(ctx.paintGlobPrefab, ctx.player.position + Vector3.up * 1.3f, Quaternion.identity);
                var glob = go.GetComponent<PaintGlob>() ?? go.AddComponent<PaintGlob>();
                glob.Init(t.transform, ctx.globSpeed, HitSource.PaintLauncherGlob, ctx.enemyLayer);
            }
        }
    }
}

