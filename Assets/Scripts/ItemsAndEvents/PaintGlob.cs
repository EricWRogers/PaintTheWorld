// PaintGlob.cs
using UnityEngine;
using SuperPupSystems.Helper;

[RequireComponent(typeof(SphereCollider))]
public class PaintGlob : MonoBehaviour
{
    public int damage = 15;
    public float speed = 18f;
    public float turnRate = 720f;
    public float lifetime = 5f;
    public HitSource source;
    public LayerMask enemyLayer;

    private Transform target;
    private float t;

    public void Init(Transform target, float speed, HitSource src, LayerMask layer)
    {
        this.target = target;
        this.speed = speed;
        this.source = src;
        this.enemyLayer = layer;

        var col = GetComponent<SphereCollider>();
        col.isTrigger = true;
        if (col.radius < 0.1f) col.radius = 0.15f;
    }

    private void Update()
    {
        t += Time.deltaTime;
        if (t > lifetime || target == null) { Destroy(gameObject); return; }

        var to = (target.position + Vector3.up * 0.8f) - transform.position;
        if (to.sqrMagnitude > 0.001f)
        {
            var q = Quaternion.LookRotation(to.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, q, turnRate * Time.deltaTime);
        }
        transform.position += transform.forward * speed * Time.deltaTime;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & enemyLayer) == 0) return;

        var go = other.attachedRigidbody ? other.attachedRigidbody.gameObject : other.gameObject;
        var hp = go.GetComponent<Health>();
        if (hp != null) hp.Damage(damage);

        // visual paint on impact if using Paintable
        var p = go.GetComponent<Paintable>();
        if (p != null) PaintManager.instance.paint(p, other.ClosestPoint(transform.position), 0.5f, 0.7f, 1f, Color.red);

        GameEvents.PlayerHitEnemy?.Invoke(go, damage, source);
        Destroy(gameObject);
    }
}
