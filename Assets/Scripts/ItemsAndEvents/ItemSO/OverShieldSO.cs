using UnityEngine;

[CreateAssetMenu(fileName="Overshield", menuName="Items/Rare/Overshield")]
public class OvershieldSO : ItemSO
{
    [Header("Shield Tuning")]
    public int baseCapacity = 25;          
    public int capacityPerStack = 15;      
    public bool fullOnEquip = true;       

    public override void OnEquipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;
        var shield = ctx.player.GetComponent<OvershieldController>();
        if (!shield) shield = ctx.player.gameObject.AddComponent<OvershieldController>();

        int cap = Mathf.Max(0, baseCapacity + capacityPerStack * Mathf.Max(0, count - 1));
        shield.Capacity = cap;
        if (fullOnEquip) shield.Current = cap;

        
        shield.RaiseUI();
    }

    public override void OnUnequipped(PlayerContext ctx, int count)
    {
        if (!ctx.player) return;
        var shield = ctx.player.GetComponent<OvershieldController>();
        if (!shield) return;

        
        int stacks = PlayerManager.instance ? PlayerManager.instance.inventory.GetCount(id) : 0;
        if (stacks <= 0)
        {
            Object.Destroy(shield);
        }
        else
        {
            int cap = Mathf.Max(0, baseCapacity + capacityPerStack * Mathf.Max(0, stacks - 1));
            shield.Capacity = cap;
            shield.Current = Mathf.Min(shield.Current, cap);
            shield.RaiseUI();
        }
    }
}

