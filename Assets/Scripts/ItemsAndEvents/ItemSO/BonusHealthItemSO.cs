using UnityEngine;

[CreateAssetMenu(fileName = "BonusHealthItem", menuName = "Items/Common/Bonus Health")]
public class BonusHealthItemSO : ItemSO
{
    [Header("Health Bonus")]
    public int healthPerStack = 1;

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (PlayerManager.instance == null) return;

        int stacks = Mathf.Max(1, count);
        int totalBonus = healthPerStack * stacks;

        PlayerManager.instance.SetBonusHealthFromItems(totalBonus);
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (PlayerManager.instance == null) return;
        PlayerManager.instance.SetBonusHealthFromItems(0);
    }
}