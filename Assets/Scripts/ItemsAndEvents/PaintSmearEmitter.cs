using UnityEngine;

[DisallowMultipleComponent]
public class PaintSmearEmitter : MonoBehaviour
{
    [Header("Smear Timing")]
    public float smearDuration = 1.0f;
    public float tickInterval = 0.05f;

    [Header("Smear Size")]
    public float baseRadius = 0.9f;
    public float radiusPerStack = 0.3f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Directional Smear")]
    public float forwardStampDistance = 0.5f;
    public float sideStampOffset = 0.4f;

    [Header("Ground Detection")]
    public float rayStartOffset = 0.7f;
    public float rayDistance = 3f;
    public LayerMask paintMask = ~0;

    private PlayerMovement _movement;
    private PlayerPaint _playerPaint;
    private float _timer;
    private float _tickTimer;
    private int _stacks = 1;

    private bool _smearing = false;
    private Color _smearColor = Color.white;
    private Color _lastStandingColor = Color.clear;

    private Vector3 _lastPos;

    void Awake()
    {
        _movement = GetComponent<PlayerMovement>()
                 ?? GetComponentInParent<PlayerMovement>()
                 ?? GetComponentInChildren<PlayerMovement>(true);

        _playerPaint = GetComponent<PlayerPaint>()
                    ?? GetComponentInParent<PlayerPaint>()
                    ?? GetComponentInChildren<PlayerPaint>(true);

        _lastPos = transform.position;
    }

    public void Configure(int stacks, float duration, float baseR, float radiusStack)
    {
        _stacks = Mathf.Max(1, stacks);
        smearDuration = duration;
        baseRadius = baseR;
        radiusPerStack = radiusStack;
    }

    void Update()
    {
        if (_movement == null || _movement.standPaintColor == null) return;

        Color standingColor = _movement.standPaintColor.standingColor;

        // Detect stepping into a different paint color
        if (!ColorsApproximatelyEqual(standingColor, _lastStandingColor))
        {
           
            if (IsValidPaintColor(standingColor))
            {
                StartSmear(standingColor);
            }

            _lastStandingColor = standingColor;
        }

        if (_smearing)
        {
            _timer -= Time.deltaTime;
            _tickTimer += Time.deltaTime;

            if (_tickTimer >= tickInterval)
            {
                _tickTimer = 0f;
                EmitSmearStamp();
            }

            if (_timer <= 0f)
            {
                _smearing = false;
            }
        }

        _lastPos = transform.position;
    }

    void StartSmear(Color groundColor)
    {
        _smearColor = groundColor;
        _timer = smearDuration;
        _tickTimer = 0f;
        _smearing = true;
    }

    void EmitSmearStamp()
    {
        if (!PaintManager.instance) return;

        float r = baseRadius + radiusPerStack * (_stacks - 1);

        Vector3 moveDir = transform.position - _lastPos;
        moveDir.y = 0f;

        if (moveDir.sqrMagnitude < 0.0001f)
            moveDir = transform.forward;
        else
            moveDir.Normalize();

        Vector3 right = Vector3.Cross(Vector3.up, moveDir).normalized;

        // Center + forward smear
        StampAt(transform.position, r, _smearColor);
        StampAt(transform.position + moveDir * forwardStampDistance, r, _smearColor);

        // Side widening
        StampAt(transform.position + right * sideStampOffset, r, _smearColor);
        StampAt(transform.position - right * sideStampOffset, r, _smearColor);
    }

    void StampAt(Vector3 worldPoint, float radius, Color color)
    {
        Vector3 origin = worldPoint + Vector3.up * rayStartOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, paintMask, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (p)
            {
                PaintManager.instance.paint(p, hit.point, radius, hardness, strength, color);
                return;
            }
        }

        Collider[] cols = Physics.OverlapSphere(worldPoint, 0.75f, paintMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var p = c.GetComponent<Paintable>() ?? c.GetComponentInParent<Paintable>();
            if (!p) continue;

            Vector3 pt = c.ClosestPoint(worldPoint);
            PaintManager.instance.paint(p, pt, radius, hardness, strength, color);
            return;
        }
    }

    bool IsValidPaintColor(Color c)
    {
        
        return c.a > 0.01f;
    }

    bool ColorsApproximatelyEqual(Color a, Color b, float tolerance = 0.02f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance &&
               Mathf.Abs(a.a - b.a) < tolerance;
    }
}