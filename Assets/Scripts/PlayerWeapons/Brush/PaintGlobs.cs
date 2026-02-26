using System;
using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public float launchForce = 15f;
    private Rigidbody rb;
    [HideInInspector] public float damageMult => PlayerManager.instance.stats.skills[1].currentMult;


    public new void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) 
            rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
        Renderer blobRenderer = gameObject.GetComponent<Renderer>();
        PlayerPaint paintComponent = PlayerManager.instance?.player?.GetComponent<PlayerPaint>();
        if (blobRenderer != null && paintComponent != null)
        {
            blobRenderer.material.SetColor("_Paint_Color", paintComponent.selectedPaint);
        }
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
}