
using UnityEngine;
using SuperPupSystems.Helper;

public static class PaintExplosion
{
    public static void DoDamageCircle(Vector3 center, float radius, int damage, LayerMask enemyLayer)
    {
        foreach (var h in Physics.OverlapSphere(center, radius, enemyLayer))
        {
            var go = h.attachedRigidbody ? h.attachedRigidbody.gameObject : h.gameObject;
            var hp = go ? go.GetComponent<Health>() : null;
            if (hp != null)
            {
                hp.Damage(damage);
                GameEvents.PlayerHitEnemy?.Invoke(go, damage, HitSource.PlayerWeapon);
            }

            
            var p = go ? go.GetComponent<Paintable>() : null;
            if (p != null) PaintManager.instance.paint(p, go.transform.position, 0.6f, 0.7f, 1f, Color.red);
        }
    }
}

