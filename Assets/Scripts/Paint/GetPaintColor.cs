using System.Collections.Generic;
using UnityEngine;

public class GetPaintColor : MonoBehaviour
{
    public Color standingColor;
    public LayerMask ignoreMask;
    RenderTexture activeMask;
    private Texture2D readableTexture;
    void Awake()
    {
        readableTexture = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
    }

    void Update()
    {
        CheckOnPaint();
    }
    public void CheckOnPaint()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 3f, ~ignoreMask))
        {
            Paintable temp = hit.transform.gameObject.GetComponent<Paintable>();
            if (temp == null) return;
            activeMask = null;

            activeMask = temp.getMask();

            if (activeMask)
            {
                RenderTexture.active = activeMask;
                readableTexture.ReadPixels(new Rect(0, 0, activeMask.width, activeMask.height), 0, 0);
                readableTexture.Apply();
                int pixelX = Mathf.FloorToInt(hit.textureCoord.x * activeMask.width);
                int pixelY = Mathf.FloorToInt(hit.textureCoord.y * activeMask.height);
                standingColor = readableTexture.GetPixel(pixelX, pixelY);
            }
        }

    }
}
