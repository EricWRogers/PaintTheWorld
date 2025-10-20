using UnityEngine;

[CreateAssetMenu(fileName = "ReactiveArmor", menuName = "Items/Reactive Armor")]
public class ReactiveArmorSO : ItemSO
{
    public int damage = 25;
    public float baseRadius = 3f, radiusPerStack = 1f;

    public override void OnPlayerDamaged(PlayerContext ctx, int damageTaken, int count)
    {
        if (!ctx.player) return;
        float r = baseRadius + radiusPerStack * (count - 1);
        //different than paint explosion 
        var cols = Physics.OverlapSphere(ctx.player.position, r, ctx.enemyLayer);
        foreach (var c in cols)
        {
            var h = c.GetComponent<SuperPupSystems.Helper.Health>();
            if (h) h.Damage(damage);
        }
    }
}
