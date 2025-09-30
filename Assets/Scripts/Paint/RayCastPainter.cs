using UnityEngine;

public class RayCastPainter : MonoBehaviour
{
    public Color paintColor;

    public float rayDistance;
    public Vector3 rayDirection;
    public LayerMask layerMask;
    public Vector3 rayOffset;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;
    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position + rayOffset, rayDirection, out hit, rayDistance, layerMask))
        {
            Paintable p = hit.collider.GetComponent<Paintable>();
            if(p != null){
                PaintManager.instance.paint(p, hit.point, radius, hardness, strength, paintColor);
            }
        }
    }
}
