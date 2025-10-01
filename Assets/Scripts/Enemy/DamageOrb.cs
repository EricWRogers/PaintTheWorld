using SuperPupSystems.Helper;
using UnityEngine;

public class DamageOrb : MonoBehaviour
{
    public float radius;
    public int damage;
    public LayerMask layerMask;
    public float orbTimer;
    private float m_timer;
    void Start()
    {
        m_timer = orbTimer;
    }

    void Update()
    {
        m_timer -= Time.deltaTime;
        if (Physics.OverlapSphere(transform.position, radius, layerMask).Length > 0 && m_timer < 0)
        {
            Collider[] hits = Physics.OverlapSphere(transform.position, radius, layerMask);
            foreach (Collider hit in hits)
            {
                Health health = PlayerManager.instance.health;
                if (health != null)
                {
                    health.Damage(damage);
                    Destroy(gameObject);
                }
            }
        }
        if (m_timer < 0)
        {
            Destroy(gameObject);
        }
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}
