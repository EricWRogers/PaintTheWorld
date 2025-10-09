using UnityEngine;

[CreateAssetMenu(fileName = "ReactiveArmor", menuName = "Items/Reactive Armor")]
public class ReactiveArmorSO : ItemSO
{
    [Header("Explosion")]
    public int damage = 25;
    public float baseRadius = 3f;
    public float radiusPerStack = 1f;

    public override void OnPlayerDamaged(PlayerContext ctx, int damageTaken, int count)
    {
        if (!ctx.player) return;
        float r = baseRadius + radiusPerStack * (count - 1);
        PaintExplosion.DoDamageCircle(ctx.player.position, r, damage, ctx.enemyLayer);
    }
}
