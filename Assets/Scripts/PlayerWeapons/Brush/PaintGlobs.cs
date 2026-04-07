using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    [Header("Projectile Settings")]
    public float damage = 10f;
    public float customGravityMultiplier = 0.08f;

    private Rigidbody rb;
    private Color blobColor = Color.white;

    public void InitializeProjectile(Rigidbody projectileRb)
    {
        rb = projectileRb;
    }

    private void FixedUpdate()
    {
        if (rb != null)
        {
            rb.AddForce(Physics.gravity * customGravityMultiplier, ForceMode.Acceleration);
        }
    }

    private void Update()
    {
        var selected =
            PlayerManager.instance.player.GetComponent<PlayerMovement>().standPaintColor.selectedPaint;

        if (blobColor != selected)
        {
            blobColor = selected;

            Renderer renderer = GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = blobColor;
            }
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.GetComponent<Health>().Damage((int)damage);
                Debug.Log($"Enemy hit! Dealt {damage} damage.");
            }
        }
    }
}