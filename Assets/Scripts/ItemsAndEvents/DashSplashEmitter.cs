using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class DashSplashEmitter : MonoBehaviour
{
    [Header("Trail Tuning")]
    public float baseRadius = 0.9f;
    public float radiusPerStack = 0.3f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Sampling")]
    public float tickInterval = 0.05f;
    public float rayStartOffset = 0.6f;
    public float rayDistance = 2.5f;
    public LayerMask paintMask = ~0;

    private int _stacks = 1;
    private float _timer = 0f;
    private bool _emitting = false;
    private Coroutine _emitRoutine;

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

    void Update()
    {
        if (!_emitting) return;

        _timer += Time.deltaTime;
        if (_timer >= tickInterval)
        {
            _timer = 0f;
            DropPaint();
        }
    }

    public void Configure(int stacks)
    {
        _stacks = Mathf.Max(1, stacks);
    }

    public void EmitForDuration(int stacks, float duration)
    {
        Configure(stacks);

        if (_emitRoutine != null)
            StopCoroutine(_emitRoutine);

        _emitRoutine = StartCoroutine(EmitRoutine(duration));
    }

    IEnumerator EmitRoutine(float duration)
    {
        _emitting = true;
        _timer = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        _emitting = false;
        _emitRoutine = null;
    }

    void DropPaint()
    {
        if (!PaintManager.instance) return;

        Color color = GetCurrentPaintColor();
        float r = baseRadius + radiusPerStack * (_stacks - 1);

        Vector3 origin = transform.position + Vector3.up * rayStartOffset;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, rayDistance, paintMask, QueryTriggerInteraction.Ignore))
        {
            var p = hit.collider.GetComponent<Paintable>() ?? hit.collider.GetComponentInParent<Paintable>();
            if (p)
            {
                PaintManager.instance.paint(p, hit.point, r, hardness, strength, color);
                return;
            }
        }

        Collider[] cols = Physics.OverlapSphere(transform.position, 0.75f, paintMask, QueryTriggerInteraction.Ignore);
        foreach (var c in cols)
        {
            var p = c.GetComponent<Paintable>() ?? c.GetComponentInParent<Paintable>();
            if (!p) continue;

            Vector3 pt = c.ClosestPoint(transform.position);
            PaintManager.instance.paint(p, pt, r, hardness, strength, color);
            return;
        }
    }

    Color GetCurrentPaintColor()
    {
        if (_playerPaint) return _playerPaint.selectedPaint;
        if (_movement && _movement.standPaintColor != null) return _movement.standPaintColor.selectedPaint;
        return Color.white;
    }
}