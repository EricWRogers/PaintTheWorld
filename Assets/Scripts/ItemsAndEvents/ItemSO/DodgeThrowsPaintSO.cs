using UnityEngine;

[CreateAssetMenu(fileName="DodgeThrowsPaint", menuName="Items/Common/Dodge Throws Paint")]
public class DodgeThrowsPaintSO : ItemSO
{
    public int globsPerStack = 1;

    public override void OnDodged(PlayerContext ctx, int count)
    {
        if (!ctx.paintGlobPrefab || !ctx.player) return;
        int total = Mathf.Max(1, globsPerStack) * Mathf.Max(1, count);
        var targets = EnemyFinder.FindClosest(ctx.player.position, total, ctx.enemyLayer);
        foreach (var t in targets)
        {
            var go = Object.Instantiate(ctx.paintGlobPrefab, ctx.player.position + Vector3.up * 1.2f, Quaternion.identity);
            var glob = go.GetComponent<PaintGlob>() ?? go.AddComponent<PaintGlob>();
            glob.Init(t.transform, ctx.globSpeed, HitSource.GiftOfLifeGlob, ctx.enemyLayer);
        }
    }
}
