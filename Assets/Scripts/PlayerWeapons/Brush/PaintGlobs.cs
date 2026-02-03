using System;
using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public float launchForce = 15f;
    private Rigidbody rb;
    [HideInInspector] public float damageMult => PlayerManager.instance.stats.skills[1].currentMult;


    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) 
            rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
        Renderer blobRenderer = gameObject.GetComponent<Renderer>();
        blobRenderer.material.SetColor("_BaseColor", PlayerManager.instance.player.GetComponent<PlayerPaint>().selectedPaint);
    }


    new void OnCollisionStay(Collision other)
    {
        if (Physics.OverlapSphere(transform.position, radius).Length > 0)
        {
           
            Collider[] hits = Physics.OverlapSphere(transform.position, radius);
            foreach (Collider hit in hits)
            {
                Paint(other);
            }
            Destroy(gameObject);
        }

        Destroy(gameObject);

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}