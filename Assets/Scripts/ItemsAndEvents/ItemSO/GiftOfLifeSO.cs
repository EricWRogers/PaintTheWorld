using UnityEngine;

[CreateAssetMenu(fileName = "GiftOfLife", menuName = "Items/Gift of Life")]
public class GiftOfLifeSO : ItemSO
{
    public int globsPerStack = 2;

    public override void OnPlayerHealed(PlayerContext ctx, int healAmount, int count)
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
