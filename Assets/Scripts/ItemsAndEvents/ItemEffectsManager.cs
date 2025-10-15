using UnityEngine;

public class ItemEffectsManager : MonoBehaviour
{
    private Inventory inv;
    private PlayerManager pm;

    void OnEnable()
    {
        pm = PlayerManager.instance;
        if (!pm) return;

        inv = pm.inventory;
        if (inv != null) inv.onChanged.AddListener(ReapplyEquippedEffects);

        // Also reapply after loads
        ReapplyEquippedEffects();
    }

    void OnDisable()
    {
        if (inv != null) inv.onChanged.RemoveListener(ReapplyEquippedEffects);
    }

    public void ReapplyEquippedEffects()
    {
        if (!pm || pm.player == null) return;

        // reset to default targets
        var scaler = pm.player.GetComponent<PlayerPaintWidthScaler>();
        if (scaler) scaler.widthMultiplier = 1f;

        var jumps = pm.player.GetComponent<ExtraJumpController>();
        if (jumps) jumps.extraJumpsGranted = 0;

        // Now let each item apply
        var ctx = pm.GetContext();
        var list = pm.inventory.items;

        for (int i = 0; i < list.Count; i++)
        {
            var s = list[i];
            if (s?.item == null || s.count <= 0) continue;
            s.item.OnEquipped(ctx, s.count);
        }
    }
}
