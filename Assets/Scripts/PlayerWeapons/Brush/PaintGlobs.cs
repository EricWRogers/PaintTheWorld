using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    [Header("Projectile Settings")]
    public float damage = 10f;
    public float customGravityMultiplier = 0.08f;

    private Rigidbody rb;
    private Color blobColor = Color.white;

    public ParticleSystem paintImpact;

    void Start()
    {
        if (paintImpact != null)
        {
            paintImpact.Stop();
        }
    }

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

        if (paintImpact != null)
        {
            var main = paintImpact.main;
            main.startColor = blobColor;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (paintImpact != null)
        {
            paintImpact.transform.position = collision.contacts[0].point;
            paintImpact.transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal);
        
            paintImpact.transform.SetParent(null); 
        
            paintImpact.Play();
            Destroy(paintImpact.gameObject, paintImpact.main.duration);
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.GetComponent<Health>().Damage((int)damage);
                Debug.Log($"Enemy hit! Dealt {damage} damage.");
            }
        }

        Destroy(gameObject);
    }
}