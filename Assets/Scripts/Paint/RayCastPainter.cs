using UnityEngine;

public class RayCastPainter : MonoBehaviour
{
    public Color paintColor;

    public Transform rayStart;
    public float rayDistance;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;
 

    [HideInInspector]
    public Ray rayCast;
    [HideInInspector]
    public RaycastHit hit;

    public void TryPaint(RaycastHit hitInfo)
    {
        Paintable p = hitInfo.collider.GetComponent<Paintable>();
        if (p != null)
        {
            Debug.Log(hitInfo.transform.name + radius + paintColor);
            PaintManager.instance.paint(p, hitInfo.point, radius, hardness, strength, paintColor);
        }
    }
}
