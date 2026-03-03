using System;
using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public float launchForce = 15f;
    private Rigidbody rb;
    private Color blobColor = Color.magenta;

    public new void Start()
    {
        rb = GetComponent<Rigidbody>();
        if (rb != null) 
            rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);

        var player = PlayerManager.instance?.player;
        if (player != null)
        {
            var paintComp = player.GetComponent<PlayerPaint>();
            if (paintComp != null)
            {
                InitializeGlob(paintComp.selectedPaint);
            }
        }
    }

    public void InitializeGlob(Color color)
    {
        blobColor = color;
        Renderer blobRenderer = GetComponent<Renderer>();
        if (blobRenderer != null)
        {
            blobRenderer.material.SetColor("_Paint_Color", blobColor);
        }
    }

    new void OnCollisionStay(Collision other)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, 0.5f);

        foreach (var hit in hits)
        {
            var p = hit.GetComponent<Paintable>();
            if (!p) continue;

            Vector3 pos = hit.ClosestPoint(transform.position);
            
            if (PaintManager.instance != null)
            {
                PaintManager.instance.paint(p, pos, 1f, hardness, strength, blobColor);
            }
        }

        Destroy(gameObject);
    }
}