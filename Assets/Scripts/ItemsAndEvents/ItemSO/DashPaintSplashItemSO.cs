using UnityEngine;

[CreateAssetMenu(menuName = "Items/Dash Paint Splash")]
public class DashPaintSplashItemSO : ItemSO
{
    [Header("Splash Tuning")]
    public float baseRadius = 1.5f;
    public float radiusPerStack = 0.75f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Optional VFX")]
    public GameObject splashVfxPrefab;

    public override void OnDodged(PlayerContext ctx, int count)
    {
        if (ctx.player == null) return;

        
        float r = baseRadius + radiusPerStack * (Mathf.Max(1, count) - 1);

        
        Vector3 center = ctx.player.position;

        
        Color color = PaintBurstUtil.CurrentPlayerPaintColor(ctx.player.gameObject);

        // paint
        PaintBurstUtil.PaintCircle(center, r, color, hardness, strength);

        
        if (splashVfxPrefab != null)
        {
            var vfx = GameObject.Instantiate(splashVfxPrefab, center, Quaternion.identity);

            
            var painter = vfx.GetComponentInChildren<ParticlePainter>();
            if (painter != null) painter.UpdateColorFromManager();

            GameObject.Destroy(vfx, 2.0f);
        }
    }
}