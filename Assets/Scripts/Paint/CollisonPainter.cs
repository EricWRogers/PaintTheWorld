using UnityEngine;
using SuperPupSystems.Helper;

public class CollisonPainter : MonoBehaviour
{
    public Color paintColor;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    public void OnCollisionStay(Collision other)
    {
        Paint(other);
    }
    public void Paint(Collision other)
    {
        Debug.Log("paint" + other.gameObject.name);
        Paintable p = other.collider.GetComponent<Paintable>();
        if (p != null)
        {
            Vector3 pos = other.contacts[0].point;
            PaintManager.instance.paint(p, pos, radius, hardness, strength, paintColor);
        }
    }
}
