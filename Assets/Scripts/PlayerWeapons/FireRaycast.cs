using UnityEngine;

public class FireRaycast : MonoBehaviour
{
    public RaycastHit hitInfo;
    public float length;
    public LayerMask layerMask;
    public Ray ray;
    public bool hit;
    void Update()
    {
        ray = new Ray(transform.position, -transform.forward);
        transform.LookAt(Camera.main.transform.position);
        if (Physics.Raycast(ray, out hitInfo, length, layerMask))
        {
            hit = true;
        }
        else
        {
            hit = false;
        }
    }
}
