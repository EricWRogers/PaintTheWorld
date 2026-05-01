using UnityEngine;
using SuperPupSystems.Helper;

public class CollisonPainter : MonoBehaviour
{
    public Color paintColor;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    [Header("Dirty Spot Rules")]
    [Range(0f, 1f)]
    public float minimumGroundDot = 0.6f;

    public void OnCollisionEnter(Collision other)
    {
        paintColor = PlayerManager.instance.player.GetComponent<PlayerMovement>().standPaintColor.selectedPaint;
        Paint(other);
    }

    public void Paint(Collision other)
    {
        Paintable p = other.collider.GetComponent<Paintable>();
        if (p == null)
        {
            p = other.collider.GetComponentInParent<Paintable>();
        }

        if (p != null)
        {
            Vector3 pos = other.contacts[0].point;
            Vector3 normal = other.contacts[0].normal;

            PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColor);

            // Only register dirty spots on floors
            float groundDot = Vector3.Dot(normal.normalized, Vector3.up);
            if (groundDot >= minimumGroundDot)
            {
                if (PaintSpotManager.instance != null)
                {
                    PaintSpotManager.instance.RegisterDirtySpot(pos);
                }
            }
        }

        Destroy(gameObject);
    }
}