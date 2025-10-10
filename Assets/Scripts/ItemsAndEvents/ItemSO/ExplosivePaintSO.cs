using UnityEngine;

[CreateAssetMenu(fileName = "ExplosivePaint", menuName = "Items/Explosive Paint")]
public class ExplosivePaintSO : ItemSO
{
    [Header("Explosion")]
    public int damage = 35;
    public float baseRadius = 3f;
    public float radiusPerStack = 1f;

    public override void OnEnemyKilled(PlayerContext ctx, GameObject enemy, int count)
    {
        if (!enemy) return;
        float r = baseRadius + radiusPerStack * (count - 1);
        PaintExplosion.DoDamageCircle(enemy.transform.position, r, damage, ctx.enemyLayer);
    }
}

