using UnityEngine;

public class WallPaintDrip : MonoBehaviour
{
    [HideInInspector] public Paintable paintable;
    [HideInInspector] public Vector3 wallNormal;
    [HideInInspector] public Color color = Color.white;
    [HideInInspector] public float radius = 0.4f;
    [HideInInspector] public float hardness = 1f;
    [HideInInspector] public float strength = 1f;
    [HideInInspector] public float duration = 0.75f;
    [HideInInspector] public float speed = 1.8f;

    private float _timeLeft;
    private float _tickTimer;

    void Start()
    {
        _timeLeft = duration;
    }

    void Update()
    {
        if (!PaintManager.instance || paintable == null)
        {
            Destroy(gameObject);
            return;
        }

        _timeLeft -= Time.deltaTime;
        _tickTimer += Time.deltaTime;

        // Move downward
        transform.position += Vector3.down * speed * Time.deltaTime;
        transform.position += wallNormal * 0.002f;

        if (_tickTimer >= 0.05f)
        {
            _tickTimer = 0f;
            PaintManager.instance.paint(paintable, transform.position, radius, hardness, strength, color);
        }

        if (_timeLeft <= 0f)
            Destroy(gameObject);
    }
}