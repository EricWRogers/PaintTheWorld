using UnityEngine;
using SuperPupSystems.Helper;

public class EnemyBullet : MonoBehaviour
{
    public float radius;
    public void OnImpact()
    {
        var hits = Physics.OverlapSphere(transform.position, radius);
        if (hits.Length > 0)
        {
            foreach (Collider hit in hits)
            {
                Paintable tempPaintable = hit.transform.gameObject.GetComponent<Paintable>();
                if (tempPaintable != null)
                {
                    PaintManager.instance.paint(tempPaintable, hit.ClosestPoint(transform.position), radius, 1f, 1f, Color.clear);
                }
                if(hit.transform.gameObject.CompareTag("Player"))
                {
                    PlayerManager.instance.health.Damage(1);
                }
            }
        }
    }
}
