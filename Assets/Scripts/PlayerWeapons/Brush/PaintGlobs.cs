using System;
using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public float launchForce = 15f;
    private Rigidbody rb;
    private Color blobColor = Color.white;

    private float damage = 10f;

    public new void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) 
            rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
    }


    void Update()
    {
        if (blobColor != PlayerManager.instance.player.GetComponent<PlayerMovement>().standPaintColor.selectedPaint){
            blobColor = PlayerManager.instance.player.GetComponent<PlayerMovement>().standPaintColor.selectedPaint;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.GetComponent<Health>().Damage((int)damage);
            }
        }
    }
}