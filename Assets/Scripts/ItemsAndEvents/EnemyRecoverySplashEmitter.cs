using UnityEngine;

public static class EnemyRecoverySplashEmitter
{
    public static void Emit(
        PlayerContext ctx,
        GameObject enemy,
        float basePaintRadius,
        float radiusPerStack,
        int baseGlobCount,
        int globCountPerStack,
        float hardness,
        float strength,
        float spawnHeight,
        float globRingRadius,
        float outwardForceMultiplier,
        int stacks)
    {
        if (enemy == null || !PaintManager.instance) return;

        int clampedStacks = Mathf.Max(1, stacks);
        float paintRadius = basePaintRadius + radiusPerStack * (clampedStacks - 1);
        int globCount = baseGlobCount + globCountPerStack * (clampedStacks - 1);

        Color color = PaintBurstUtil.CurrentPlayerPaintColor(ctx.player ? ctx.player.gameObject : null);

        // Ground splash
        if (TryGetGroundPoint(enemy.transform.position, out Vector3 groundPoint))
        {
            PaintBurstUtil.PaintCircle(groundPoint, paintRadius, color, hardness, strength);
            SpawnGlobRing(ctx, groundPoint, globCount, spawnHeight, globRingRadius, outwardForceMultiplier);
        }
        else
        {
            // No floor found, still burst for flying enemies later
            SpawnGlobRing(ctx, enemy.transform.position, globCount, spawnHeight, globRingRadius, outwardForceMultiplier);
        }
    }

    static bool TryGetGroundPoint(Vector3 originWorld, out Vector3 groundPoint)
    {
        Vector3 origin = originWorld + Vector3.up * 4f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 20f, ~0, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (p)
            {
                groundPoint = hit.point;
                return true;
            }
        }

        groundPoint = originWorld;
        return false;
    }

    static void SpawnGlobRing(
        PlayerContext ctx,
        Vector3 center,
        int count,
        float spawnHeight,
        float globRingRadius,
        float outwardForceMultiplier)
    {
        if (ctx.player == null) return;

        var spray = ctx.player.GetComponentInChildren<SprayPaintLine>(true);
        if (spray == null || spray.projectilePrefab == null) return;

        Vector3 spawnCenter = center + Vector3.up * spawnHeight;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            Vector3 spawnPos = spawnCenter + dir * globRingRadius;
            Quaternion rot = Quaternion.LookRotation(dir);

            GameObject proj = Object.Instantiate(spray.projectilePrefab, spawnPos, rot);

            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb == null) rb = proj.AddComponent<Rigidbody>();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            float force = (spray.launchForce * 0.01f) * outwardForceMultiplier;
            rb.AddForce(dir * force + Vector3.up * 1.2f, ForceMode.Impulse);

            Object.Destroy(proj, spray.projectileLifetime);
        }
    }
}