// DebugRayWeapon.cs
using UnityEngine;
using SuperPupSystems.Helper;

public class DebugRayWeapon : MonoBehaviour
{
    public Camera cam;
    public float range = 50f;
    public int damage = 10;
    public LayerMask enemyLayer;

    private void Awake()
    {
        if (!cam) cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0)) // left click
        {
            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out var hit, range, enemyLayer))
            {
                var go = hit.collider.attachedRigidbody ? hit.collider.attachedRigidbody.gameObject : hit.collider.gameObject;
                var hp = go.GetComponent<Health>();
                if (hp != null)
                {
                    hp.Damage(damage);
                    GameEvents.PlayerHitEnemy?.Invoke(go, damage, HitSource.PlayerWeapon);
                }
            }
        }
    }
}

