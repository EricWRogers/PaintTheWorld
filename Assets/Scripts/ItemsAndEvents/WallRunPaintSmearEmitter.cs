using UnityEngine;

[DisallowMultipleComponent]
public class WallRunPaintSmearEmitter : MonoBehaviour
{
    [Header("Smear")]
    public float tickInterval = 0.05f;
    public float baseRadius = 0.7f;
    public float radiusPerStack = 0.2f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Wall Sampling")]
    public float wallRayDistance = 1.5f;
    public float verticalOffset = 1.0f;
    public float wallSurfaceOffset = 0.05f;

    [Header("Drips")]
    public int baseDripCount = 2;
    public int dripCountPerStack = 1;
    public float dripDuration = 0.75f;
    public float dripSpeed = 1.8f;
    public float dripSpacing = 0.3f;
    public float dripRadiusMultiplier = 0.7f;

    private PlayerMovement _movement;
    private PlayerPaint _playerPaint;
    private float _timer;
    private int _stacks = 1;

    void Awake()
    {
        _movement = GetComponent<PlayerMovement>()
                 ?? GetComponentInParent<PlayerMovement>()
                 ?? GetComponentInChildren<PlayerMovement>(true);

        _playerPaint = GetComponent<PlayerPaint>()
                    ?? GetComponentInParent<PlayerPaint>()
                    ?? GetComponentInChildren<PlayerPaint>(true);
    }

    public void Configure(int stacks)
    {
        _stacks = Mathf.Max(1, stacks);
    }

    void Update()
    {
        if (_movement == null) return;
        if (!_movement.m_isWallRiding) return;

        _timer += Time.deltaTime;
        if (_timer >= tickInterval)
        {
            _timer = 0f;
            SmearWall();
        }
    }

    void SmearWall()
    {
        if (!PaintManager.instance) return;

        float radius = baseRadius + radiusPerStack * (_stacks - 1);
        Color color = GetCurrentPaintColor();

        Vector3 wallNormal = _movement.GetWallNormal();
        if (wallNormal.sqrMagnitude < 0.001f) return;

        // Start just off the player, toward the wall
        Vector3 origin = transform.position + Vector3.up * verticalOffset;

        // Raycast directly into the wall
        if (Physics.Raycast(origin, -wallNormal, out RaycastHit hit, wallRayDistance, _movement.wallLayers, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (!p) return;

            PaintManager.instance.paint(p, hit.point, radius, hardness, strength, color);
            SpawnDrips(hit, color, radius);
        }
    }

    void SpawnDrips(RaycastHit wallHit, Color color, float radius)
    {
        var paintable = wallHit.collider.GetComponent<Paintable>() ?? wallHit.collider.GetComponentInParent<Paintable>();
        if (!paintable) return;

        int dripCount = baseDripCount + dripCountPerStack * (_stacks - 1);

        Vector3 wallRight = Vector3.Cross(Vector3.up, wallHit.normal).normalized;
        if (wallRight.sqrMagnitude < 0.001f)
            wallRight = Vector3.right;

        for (int i = 0; i < dripCount; i++)
        {
            float offsetT = dripCount == 1 ? 0f : Mathf.Lerp(-1f, 1f, i / (float)(dripCount - 1));
            Vector3 startPoint = wallHit.point + wallRight * (offsetT * dripSpacing) + wallHit.normal * wallSurfaceOffset;

            var dripObj = new GameObject("WallPaintDrip");
            dripObj.transform.position = startPoint;

            var drip = dripObj.AddComponent<WallPaintDrip>();
            drip.paintable = paintable;
            drip.color = color;
            drip.radius = radius * dripRadiusMultiplier;
            drip.hardness = hardness;
            drip.strength = strength;
            drip.duration = dripDuration;
            drip.speed = dripSpeed;
            drip.wallNormal = wallHit.normal;
        }
    }

    Color GetCurrentPaintColor()
    {
        if (_playerPaint) return _playerPaint.selectedPaint;
        if (_movement && _movement.standPaintColor != null) return _movement.standPaintColor.selectedPaint;
        return Color.white;
    }
}