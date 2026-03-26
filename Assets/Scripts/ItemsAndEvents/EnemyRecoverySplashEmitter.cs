using System.Collections;
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
        if (enemy == null || !PaintManager.instance || ctx.player == null) return;

        int clampedStacks = Mathf.Max(1, stacks);
        float splashRadius = basePaintRadius + radiusPerStack * (clampedStacks - 1);
        int globCount = baseGlobCount + globCountPerStack * (clampedStacks - 1);

        var playerGO = ctx.player.gameObject;
        var playerPaint = playerGO.GetComponent<PlayerPaint>() ??
                          playerGO.GetComponentInChildren<PlayerPaint>(true);

        var movement = playerGO.GetComponent<PlayerMovement>() ??
                       playerGO.GetComponentInChildren<PlayerMovement>(true);

        var spray = playerGO.GetComponentInChildren<SprayPaintLine>(true);

        if (spray == null || spray.projectilePrefab == null)
        {
            Debug.LogWarning("[EnemyRecoverySplashEmitter] Missing SprayPaintLine or projectilePrefab.");
            return;
        }

        Color color = GetCurrentPaintColor(playerPaint, movement);

        // 1) Paint the ground under the enemy
        Vector3 enemyCenter = enemy.transform.position;
        PaintGroundBurstUnderEnemy(enemyCenter, splashRadius, color, hardness, strength);

        // 2) Spawn ring of globs
        SpawnProjectileRingAboveEnemy(
            spray,
            enemy,
            enemyCenter,
            globCount,
            spawnHeight,
            globRingRadius,
            outwardForceMultiplier
        );

        Debug.Log($"[EnemyRecoverySplashEmitter] Splash emitted for {enemy.name}");
    }

    static void PaintGroundBurstUnderEnemy(
        Vector3 center,
        float splashRadius,
        Color color,
        float hardness,
        float strength)
    {
        // Center stamp
        StampGroundAt(center, splashRadius, color, hardness, strength);

        // Small ring of extra stamps
        int rings = 2;
        int samplesPerRing = 8;

        for (int ring = 1; ring <= rings; ring++)
        {
            float t = ring / (float)rings;
            float ringRadius = splashRadius * t;

            for (int i = 0; i < samplesPerRing; i++)
            {
                float angle = (Mathf.PI * 2f * i) / samplesPerRing;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ringRadius;
                StampGroundAt(center + offset, splashRadius, color, hardness, strength);
            }
        }
    }

    static void StampGroundAt(
        Vector3 worldPoint,
        float splashRadius,
        Color color,
        float hardness,
        float strength)
    {
       
        float rayStartHeight = 3.0f;
        float rayDistance = 12f;
        float stampRadius = Mathf.Max(0.45f, splashRadius * 0.35f);

        Vector3 origin = worldPoint + Vector3.up * rayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, ~0, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (p)
            {
                PaintManager.instance.paint(p, hit.point, stampRadius, hardness, strength, color);
                return;
            }
        }

        // fallback overlap
        Collider[] cols = Physics.OverlapSphere(worldPoint, 0.75f, ~0, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var p = c.GetComponent<Paintable>() ?? c.GetComponentInParent<Paintable>();
            if (!p) continue;

            Vector3 pt = c.ClosestPoint(worldPoint);
            PaintManager.instance.paint(p, pt, stampRadius, hardness, strength, color);
            return;
        }
    }

    static void SpawnProjectileRingAboveEnemy(
        SprayPaintLine spray,
        GameObject enemy,
        Vector3 enemyCenter,
        int count,
        float spawnHeight,
        float globRingRadius,
        float outwardForceMultiplier)
    {
        Vector3 spawnCenter = enemyCenter + Vector3.up * spawnHeight;

        Collider[] enemyCols = enemy.GetComponentsInChildren<Collider>(true);

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
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Ignore the recovered enemy
            Collider[] projCols = proj.GetComponentsInChildren<Collider>(true);
            foreach (var pCol in projCols)
            {
                if (!pCol) continue;
                foreach (var eCol in enemyCols)
                {
                    if (!eCol) continue;
                    Physics.IgnoreCollision(pCol, eCol, true);
                }
            }

            
            float force = (spray.launchForce * 0.01f) * outwardForceMultiplier;
            rb.AddForce(dir * force + Vector3.up * 1.25f, ForceMode.Impulse);

            Object.Destroy(proj, spray.projectileLifetime);
        }
    }

    static Color GetCurrentPaintColor(PlayerPaint playerPaint, PlayerMovement movement)
    {
        if (playerPaint) return playerPaint.selectedPaint;
        if (movement && movement.standPaintColor != null) return movement.standPaintColor.selectedPaint;
        return Color.white;
    }
}