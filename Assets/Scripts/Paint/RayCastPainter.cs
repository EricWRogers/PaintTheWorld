using UnityEngine;

public class RayCastPainter : MonoBehaviour
{
    public Color paintColor;
    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;

    void Start()
    {
        paintColor = PlayerManager.instance.player.GetComponent<PlayerPaint>().selectedPaint;
    }
    public void TryPaint(RaycastHit hitInfo)
    {
        Debug.Log(hitInfo);
        paintColor = PlayerManager.instance.player.GetComponent<PlayerPaint>().selectedPaint;
        Paintable p = hitInfo.collider.GetComponent<Paintable>();
        if (p != null)
        {
            Debug.Log(hitInfo.transform.name + radius + paintColor);
            PaintManager.instance.paint(p, hitInfo.point, radius, hardness, strength, paintColor);
        }
    }
}
