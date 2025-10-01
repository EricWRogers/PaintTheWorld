using System;
using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public int bulletDamage = 10;
    public int damage = 10;
    
    public float launchForce = 15f;
    private Rigidbody rb;


   
    public  void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null)
            rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
        Renderer blobRenderer = gameObject.GetComponent<Renderer>();
        blobRenderer.material.SetColor("_BaseColor", paintColor);
    }
    

    new void OnCollisionStay(Collision other)
    {
        
        Paint(other);
        if (other.transform.tag == "Enemy")
        {
            other.gameObject.GetComponent<Health>().Damage(bulletDamage);
        }
        Destroy(gameObject);

    }
}
