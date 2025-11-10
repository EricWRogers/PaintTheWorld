using UnityEngine;

[DisallowMultipleComponent]
public class PaintTrailEmitter : MonoBehaviour
{
    [Header("Trail Tuning")]
    public float baseRadius = 0.6f;
    public float radiusPerStack = 0.2f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Sampling")]
    public float tickInterval = 0.06f;     // how often to drop paint
    public float rayStartOffset = 0.6f;    
    public float rayDistance = 2.0f;       // how far down to look
    public LayerMask paintMask = ~0;       // what to hit for Paintable

    private float _timer;
    private int _stacks = 1;
    private PlayerPaint _playerPaint;

    public void Configure(int stacks)
    {
        _stacks = Mathf.Max(1, stacks);
    }

    void Awake()
    {
        _playerPaint = GetComponent<PlayerPaint>();
    }

    void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= tickInterval)
        {
            _timer = 0f;
            DropPaint();
        }
    }

    void DropPaint()
    {
        if (!PaintManager.instance) return;

        // Determine current paint color from player
        Color color = _playerPaint ? _playerPaint.selectedPaint : Color.white;

        // radius scales with stacks
        float r = baseRadius + radiusPerStack * (_stacks - 1);

        // cast straight down
        Vector3 origin = transform.position + Vector3.up * rayStartOffset;
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, paintMask, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (p)
            {
                PaintManager.instance.paint(p, hit.point, r, hardness, strength, color);
                // Debug.Log($"[TrailPaint] r={r:0.00} at {hit.point}");
                return;
            }
        }

        // Fallback, if not directly on a paintable, try overlapping small areea
        Collider[] cols = Physics.OverlapSphere(transform.position, 0.6f, paintMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var p = c.GetComponent<Paintable>() ?? c.GetComponentInParent<Paintable>();
            if (!p) continue;
            Vector3 pt = c.ClosestPoint(transform.position);
            PaintManager.instance.paint(p, pt, r, hardness, strength, color);
            return;
        }
    }
}
