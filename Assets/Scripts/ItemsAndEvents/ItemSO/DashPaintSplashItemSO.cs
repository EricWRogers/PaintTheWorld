using UnityEngine;

[CreateAssetMenu(menuName = "Items/Dash Paint Splash")]
public class DashPaintSplashItemSO : ItemSO
{
    [Header("Paint Splash")]
    public float baseRadius = 1.25f;
    public float radiusPerStack = 0.6f;

    [Tooltip("Lift the paint point slightly")]
    public float centerUpOffset = 0.35f;

    [Range(0f, 1f)] public float hardness = 1f;
    [Range(0f, 1f)] public float strength = 1f;

    [Header("Optional VFX")]
    public GameObject splashVfxPrefab;
    public float vfxLifetime = 2.5f;

    public override void OnDodged(PlayerContext ctx, int count)
    {
        if (ctx.player == null) return;

        // radius scales with stacks
        float radius = baseRadius + radiusPerStack * Mathf.Max(0, count - 1);

        // Use the player's current paint color
        Color color = PaintBurstUtil.CurrentPlayerPaintColor(ctx.player.gameObject); 

        Vector3 center = ctx.player.position + Vector3.up * centerUpOffset;

        // Actually paint the ground
        PaintBurstUtil.PaintCircle(center, radius, color, hardness, strength);      

    
        if (splashVfxPrefab != null)
        {
            var vfx = Instantiate(splashVfxPrefab, center, Quaternion.identity);

            
            float scale = radius / Mathf.Max(0.01f, baseRadius);
            vfx.transform.localScale *= scale;

            Destroy(vfx, vfxLifetime);
        }
    }
}