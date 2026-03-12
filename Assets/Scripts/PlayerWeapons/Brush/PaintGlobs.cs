using System;
using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public float launchForce = 15f;
    private Rigidbody rb;
    private Color blobColor = Color.white;

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
}