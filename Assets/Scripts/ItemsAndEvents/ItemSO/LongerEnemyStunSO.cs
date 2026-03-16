using UnityEngine;

[CreateAssetMenu(fileName = "LongerEnemyStun", menuName = "Items/Common/Longer Enemy Stun")]
public class LongerEnemyStunSO : ItemSO
{
    [Header("Bonuses")]
    public float baseExtraStunTime = 0.4f;
    public float extraStunPerStack = 0.2f;

    public float baseExtraGrace = 0.25f;
    public float extraGracePerStack = 0.1f;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        int stacks = Mathf.Max(1, count);

        float stunBonus = baseExtraStunTime + extraStunPerStack * (stacks - 1);
        float graceBonus = baseExtraGrace + extraGracePerStack * (stacks - 1);

        EnemyStunModifier.Configure(stunBonus, graceBonus);
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        EnemyStunModifier.ResetModifiers();
    }
}