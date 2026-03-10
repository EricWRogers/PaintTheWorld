using UnityEngine;

[DisallowMultipleComponent]
public class DashSplashEmitter : MonoBehaviour
{
    [Header("Splash Tuning")]
    public float baseRadius = 1.2f;
    public float radiusPerStack = 0.5f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Ground Sampling")]
    public float rayStartOffset = 0.75f;
    public float rayDistance = 3f;
    public LayerMask paintMask = ~0;

    [Header("Sampling Pattern")]
    public int rings = 2;
    public int samplesPerRing = 8;
    public float minStampRadius = 0.45f;
    public float stampRadiusMultiplier = 0.35f;

    private PlayerPaint _playerPaint;
    private PlayerMovement _movement;

    void Awake()
    {
        _playerPaint = GetComponent<PlayerPaint>()
                    ?? GetComponentInParent<PlayerPaint>()
                    ?? GetComponentInChildren<PlayerPaint>();

        _movement = GetComponent<PlayerMovement>()
                 ?? GetComponentInParent<PlayerMovement>()
                 ?? GetComponentInChildren<PlayerMovement>();
    }

    public void EmitBurst(int stacks)
    {
        if (!PaintManager.instance) return;

        int clampedStacks = Mathf.Max(1, stacks);
        float splashRadius = baseRadius + radiusPerStack * (clampedStacks - 1);

        Color color = GetCurrentPaintColor();

        // Center stamp
        StampAtPoint(transform.position, splashRadius, color);

        // Ring stamps
        for (int ring = 1; ring <= rings; ring++)
        {
            float t = ring / (float)rings;
            float ringRadius = splashRadius * t;

            for (int i = 0; i < samplesPerRing; i++)
            {
                float angle = (Mathf.PI * 2f * i) / samplesPerRing;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ringRadius;
                StampAtPoint(transform.position + offset, splashRadius, color);
            }
        }
    }

    private void StampAtPoint(Vector3 worldPoint, float splashRadius, Color color)
    {
        float stampRadius = Mathf.Max(minStampRadius, splashRadius * stampRadiusMultiplier);

        Vector3 origin = worldPoint + Vector3.up * rayStartOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, paintMask, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (p)
            {
                PaintManager.instance.paint(p, hit.point, stampRadius, hardness, strength, color);
                return;
            }
        }

        // Fallback overlap search like paintTrail
        Collider[] cols = Physics.OverlapSphere(worldPoint, 0.6f, paintMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var p = c.GetComponent<Paintable>() ?? c.GetComponentInParent<Paintable>();
            if (!p) continue;

            Vector3 pt = c.ClosestPoint(worldPoint);
            PaintManager.instance.paint(p, pt, stampRadius, hardness, strength, color);
            return;
        }
    }

    private Color GetCurrentPaintColor()
    {
        
        if (_playerPaint) return _playerPaint.selectedPaint;

        if (_movement && _movement.standPaintColor != null)
            return _movement.standPaintColor.selectedPaint;

        return Color.white;
    }
}