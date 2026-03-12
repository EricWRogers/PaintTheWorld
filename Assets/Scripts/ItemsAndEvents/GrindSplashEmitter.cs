using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GrindSplashEmitter : MonoBehaviour
{
    [Header("Ground Splash")]
    public float basePaintRadius = 2.0f;
    public float radiusPerStack = 0.75f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Ground Sampling")]
    public float rayStartHeight = 3.0f;
    public float rayDistance = 12f;
    public LayerMask groundMask = ~0;
    public int rings = 2;
    public int samplesPerRing = 8;
    public float minStampRadius = 0.45f;
    public float stampRadiusMultiplier = 0.35f;

    [Header("Projectile Ring")]
    public int baseGlobCount = 8;
    public int globCountPerStack = 2;
    public float spawnHeight = 0.7f;
    public float globRingRadius = 1.6f;
    public float upwardForce = 1.25f;
    public float outwardForceMultiplier = 1f;

    [Header("Projectile Safety")]
    public string projectileLayerName = "PaintSplash";
    public float colliderEnableDelay = 0.08f;
    public float railIgnoreRadius = 2.5f;

    private PlayerPaint _playerPaint;
    private PlayerMovement _movement;
    private SprayPaintLine _sprayPaintLine;
    private Collider[] _playerColliders;

    void Awake()
    {
        _playerPaint = GetComponent<PlayerPaint>()
                    ?? GetComponentInParent<PlayerPaint>()
                    ?? GetComponentInChildren<PlayerPaint>(true);

        _movement = GetComponent<PlayerMovement>()
                 ?? GetComponentInParent<PlayerMovement>()
                 ?? GetComponentInChildren<PlayerMovement>(true);

        _sprayPaintLine = GetComponent<SprayPaintLine>()
                       ?? GetComponentInChildren<SprayPaintLine>(true)
                       ?? GetComponentInParent<SprayPaintLine>();

        _playerColliders = GetComponentsInChildren<Collider>(true);
    }

    public void Emit(int stacks)
    {
        if (!PaintManager.instance) return;

        int clampedStacks = Mathf.Max(1, stacks);
        float splashRadius = basePaintRadius + radiusPerStack * (clampedStacks - 1);
        Color color = GetCurrentPaintColor();

        // 1) Paint the floor below 
        PaintGroundBurst(transform.position, splashRadius, color);

        // 2) Spawn spray projectiles in a ring
        SpawnSprayProjectileRing(transform.position, clampedStacks);
    }

    void PaintGroundBurst(Vector3 center, float splashRadius, Color color)
    {
        // Center stamp
        StampGroundAt(center, splashRadius, color);

        // Ring stamps
        for (int ring = 1; ring <= rings; ring++)
        {
            float t = ring / (float)rings;
            float ringRadius = splashRadius * t;

            for (int i = 0; i < samplesPerRing; i++)
            {
                float angle = (Mathf.PI * 2f * i) / samplesPerRing;
                Vector3 offset = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ringRadius;
                StampGroundAt(center + offset, splashRadius, color);
            }
        }
    }

    void StampGroundAt(Vector3 worldPoint, float splashRadius, Color color)
    {
        float stampRadius = Mathf.Max(minStampRadius, splashRadius * stampRadiusMultiplier);
        Vector3 origin = worldPoint + Vector3.up * rayStartHeight;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (p)
            {
                PaintManager.instance.paint(p, hit.point, stampRadius, hardness, strength, color);
                return;
            }
        }

        // Fallback overlap
        Collider[] cols = Physics.OverlapSphere(worldPoint, 0.75f, groundMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var p = c.GetComponent<Paintable>() ?? c.GetComponentInParent<Paintable>();
            if (!p) continue;

            Vector3 pt = c.ClosestPoint(worldPoint);
            PaintManager.instance.paint(p, pt, stampRadius, hardness, strength, color);
            return;
        }
    }

    void SpawnSprayProjectileRing(Vector3 centerWorld, int stacks)
    {
        if (_sprayPaintLine == null)
        {
            Debug.LogWarning("[GrindSplashEmitter] No SprayPaintLine found on player.");
            return;
        }

        if (_sprayPaintLine.projectilePrefab == null)
        {
            Debug.LogWarning("[GrindSplashEmitter] SprayPaintLine.projectilePrefab is null.");
            return;
        }

        int count = baseGlobCount + globCountPerStack * (stacks - 1);

        // Find ground beneath player so globs spawn near floor, not rail height
        Vector3 spawnCenter = centerWorld;
        if (Physics.Raycast(centerWorld + Vector3.up * rayStartHeight, Vector3.down, out RaycastHit hit, rayDistance, groundMask, QueryTriggerInteraction.Ignore))
        {
            spawnCenter = hit.point;
        }

        spawnCenter += Vector3.up * spawnHeight;

        for (int i = 0; i < count; i++)
        {
            float angle = (360f / count) * i;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            Vector3 spawnPos = spawnCenter + dir * globRingRadius;
            Quaternion rot = Quaternion.LookRotation(dir);

            GameObject proj = Instantiate(_sprayPaintLine.projectilePrefab, spawnPos, rot);

            // Put spawned projectile on dedicated safe layer
            int splashLayer = LayerMask.NameToLayer(projectileLayerName);
            if (splashLayer != -1)
                SetLayerRecursively(proj, splashLayer);
            else
                Debug.LogWarning($"[GrindSplashEmitter] Layer '{projectileLayerName}' not found.");

            // Ensure projectile has Rigidbody 
            Rigidbody rb = proj.GetComponent<Rigidbody>();
            if (rb == null) rb = proj.AddComponent<Rigidbody>();

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;

            // Ignore collisions with player and nearby rail
            IgnorePlayerCollision(proj);
            IgnoreNearbyRailCollision(proj, spawnCenter);

            // Delay collider enable 
            var projCols = proj.GetComponentsInChildren<Collider>(true);
            StartCoroutine(EnableCollidersDelayed(projCols, colliderEnableDelay));

            float force = (_sprayPaintLine.launchForce * 0.01f) * outwardForceMultiplier;
            rb.AddForce(dir * force + Vector3.up * upwardForce, ForceMode.Impulse);

            Destroy(proj, _sprayPaintLine.projectileLifetime);
        }
    }

    IEnumerator EnableCollidersDelayed(Collider[] cols, float delay)
    {
        if (cols != null)
        {
            foreach (var c in cols)
                if (c) c.enabled = false;
        }

        yield return new WaitForSeconds(delay);

        if (cols != null)
        {
            foreach (var c in cols)
                if (c) c.enabled = true;
        }
    }

    void IgnorePlayerCollision(GameObject proj)
    {
        var projCols = proj.GetComponentsInChildren<Collider>(true);
        if (projCols == null || _playerColliders == null) return;

        foreach (var pCol in projCols)
        {
            if (!pCol) continue;
            foreach (var playerCol in _playerColliders)
            {
                if (!playerCol) continue;
                Physics.IgnoreCollision(pCol, playerCol, true);
            }
        }
    }

    void IgnoreNearbyRailCollision(GameObject proj, Vector3 center)
    {
        if (_movement == null) return;

        Collider[] nearbyRails = Physics.OverlapSphere(center, railIgnoreRadius, _movement.railLayer, QueryTriggerInteraction.Ignore);
        if (nearbyRails == null || nearbyRails.Length == 0) return;

        var projCols = proj.GetComponentsInChildren<Collider>(true);
        if (projCols == null) return;

        foreach (var pCol in projCols)
        {
            if (!pCol) continue;
            foreach (var railCol in nearbyRails)
            {
                if (!railCol) continue;
                Physics.IgnoreCollision(pCol, railCol, true);
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    Color GetCurrentPaintColor()
    {
        if (_playerPaint) return _playerPaint.selectedPaint;
        if (_movement && _movement.standPaintColor != null) return _movement.standPaintColor.selectedPaint;
        return Color.white;
    }
}