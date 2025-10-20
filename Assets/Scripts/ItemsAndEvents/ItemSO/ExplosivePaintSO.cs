using UnityEngine;

[CreateAssetMenu(fileName = "ExplosivePaint", menuName = "Items/Explosive Paint")]
public class ExplosivePaintSO : ItemSO
{
    public int damage = 35;
    public float baseRadius = 3f, radiusPerStack = 1f;

    public override void OnEnemyKilled(PlayerContext ctx, GameObject enemy, int count)
    {
        if (!enemy) return;
        float r = baseRadius + radiusPerStack * (count - 1);
        var cols = Physics.OverlapSphere(enemy.transform.position, r, ctx.enemyLayer);
        foreach (var c in cols)
        {
            var h = c.GetComponent<SuperPupSystems.Helper.Health>();
            if (h) h.Damage(damage);
        }
    }
}

