using UnityEngine;
using System.Collections.Generic;




public static class PaintBurstUtil
{
    /// Paint a circular blot on nearby Paintable surfaces around a world point.
    public static void PaintCircle(Vector3 center, float paintRadius, Color color, float hardness = 1f, float strength = 1f)
    {
        if (!PaintManager.instance) return;

        // Find nearby colliders and paint on those that have a Paintable.
        var hits = Physics.OverlapSphere(center, paintRadius * 1.25f, ~0, QueryTriggerInteraction.Ignore);
        var seen = new HashSet<Paintable>();

        foreach (var col in hits)
        {
            var p = col.GetComponent<Paintable>();
            if (!p) p = col.GetComponentInParent<Paintable>();
            if (!p || seen.Contains(p)) continue;

            // Good contact point for the blot
            Vector3 point = col.ClosestPoint(center);
            if ((point - center).sqrMagnitude < 0.01f) point = center;

            PaintManager.instance.paint(p, point, paintRadius, hardness, strength, color);
            seen.Add(p);
        }

        // Fallbac, if nothing was hit, try painting the ground below with a ray
        if (seen.Count == 0 && Physics.Raycast(center + Vector3.up * 1.0f, Vector3.down, out var rh, paintRadius * 2f))
        {
            var p = rh.collider.GetComponent<Paintable>() ?? rh.collider.GetComponentInParent<Paintable>();
            if (p) PaintManager.instance.paint(p, rh.point, paintRadius, hardness, strength, color);
        }
    }

    public static Color CurrentPlayerPaintColor(GameObject player)
    {
        var pp = player ? player.GetComponent<PlayerPaint>() : null;
        return pp ? pp.selectedPaint : Color.white;
    }
}
