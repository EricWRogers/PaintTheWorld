using UnityEngine;
using SuperPupSystems.Helper;

public class CollisonPainter : MonoBehaviour
{
    public Color paintColor;

    public float radius = 1;
    public float strength = 1;
    public float hardness = 1;
    public void Start()
    {
        //paintColor = PlayerManager.instance.player.GetComponent<PlayerPaint>().selectedPaint;
    }

    public void OnCollisionStay(Collision other)
    {
        Paint(other);
    }
    public void Paint(Collision other)
    {
        if (!PaintManager.instance) return;

        var player = PlayerManager.instance ? PlayerManager.instance.player : null;

        var scaler =
            GetComponent<PlayerPaintWidthScaler>() ??
            GetComponentInParent<PlayerPaintWidthScaler>() ??
            (player ? player.GetComponentInChildren<PlayerPaintWidthScaler>(true) : null);


        var painter = player ? player.GetComponent<PlayerPaint>() : null;
        Color paintColor = painter ? painter.selectedPaint : Color.white;

        float r = radius;
        if (scaler) r = scaler.Apply(r);

        foreach (var c in other.contacts)
        {
            var p = c.otherCollider.GetComponent<Paintable>();
            if (!p) p = c.thisCollider.GetComponent<Paintable>();
            if (!p) continue;

            PaintManager.instance.paint(p, c.point, r, hardness, strength, paintColor);
            
            Debug.Log($"[TrailPaint] +{r*r*strength:0.00} at {c.point}");
        }
    }
}
