    using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Paintable))]
public class PaintableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        Paintable paintable = (Paintable)target;

        if (GUILayout.Button("Calculate Material Percentage"))
        {
            paintable.CalculateTextureCoverage();
        }

    }
}
#endif

public class Paintable : MonoBehaviour {

    public enum TextureQuality
    {
        BIG = 1024,
        MEDIUM = 512,
        SMALL = 256,
        MINI = 128,
    }
    //const int TEXTURE_SIZE = 256;
    public TextureQuality TEXTURE_SIZE = TextureQuality.MINI;

    private RenderTexture m_maskRenderTexture;
    private RenderTexture m_supportTexture;
    private Renderer m_rend;

    int maskTextureID = Shader.PropertyToID("_MaskTexture");

    public RenderTexture getMask() => m_maskRenderTexture;
    public RenderTexture getSupport() => m_supportTexture;
    public Renderer getRenderer() => m_rend;

    public bool covered;
    public float targetCoverPercent;
    public float percentageCovered;
    public float meshPercent;
    private float checkTimer = 0f;
    public float checkInterval = 0.5f;
    public bool isObj;
    

    void Start() {
        m_maskRenderTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        m_maskRenderTexture.filterMode = FilterMode.Bilinear;

        m_maskRenderTexture.useMipMap = true;
        m_maskRenderTexture.autoGenerateMips = false;
        m_maskRenderTexture.enableRandomWrite = false;
        m_maskRenderTexture.Create();

        m_supportTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        m_supportTexture.filterMode =  FilterMode.Bilinear;

        m_rend = GetComponent<Renderer>();
        m_rend.material.SetTexture(maskTextureID, m_maskRenderTexture);

        PaintManager.instance.initTextures(this);

        
        isObj = (GetComponent<PaintingObj>() != null);
        
        percentageCovered = meshPercent;
    }
    void Update()
    {
        if (isObj)
        {
            checkTimer += Time.deltaTime;

            if (checkTimer >= checkInterval)
            {
                // Update the actual percentage from the RenderTexture
                percentageCovered = GetPaintCoverage(m_maskRenderTexture);
                checkTimer = 0f;
            }
            
            if(percentageCovered >= targetCoverPercent)
            {
                covered = true;
            }
            else
            {
                covered = false;
            }
        }
        
    }

    void OnDisable(){
        m_maskRenderTexture.Release();
        m_supportTexture.Release();
    }
    public float GetPaintCoverage(RenderTexture _mask)
    {
        int mip = _mask.mipmapCount - 1;

        RenderTexture temp = RenderTexture.GetTemporary(
            1, 1, 0, _mask.format
        );

        Graphics.CopyTexture(_mask, 0, mip, temp, 0, 0);

        RenderTexture prev = RenderTexture.active;
        RenderTexture.active = temp;

        Texture2D tex = new Texture2D(1, 1, TextureFormat.R8, false);
        tex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(temp);

        return tex.GetPixel(0, 0).r * 100 + meshPercent; // 0–1 coverage
    }

    public void CalculateTextureCoverage()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        Vector2[] uvs = mesh.uv;

        int texWidth = (int)TEXTURE_SIZE;
        int texHeight = (int)TEXTURE_SIZE;
        int totalPixels = texWidth * texHeight;

        HashSet<Vector2Int> coveredPixels = new HashSet<Vector2Int>();

        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 uv1 = uvs[triangles[i]] * (int)TEXTURE_SIZE;
            Vector2 uv2 = uvs[triangles[i + 1]] * (int)TEXTURE_SIZE;
            Vector2 uv3 = uvs[triangles[i + 2]] * (int)TEXTURE_SIZE;

            RasterizeTriangle(uv1, uv2, uv3, coveredPixels, texWidth, texHeight);
        }

        meshPercent = 100 - ((float)coveredPixels.Count / totalPixels * 100f);
    }

    private void RasterizeTriangle(Vector2 p0, Vector2 p1, Vector2 p2, HashSet<Vector2Int> coveredPixels, int texWidth, int texHeight)
    {
        int minX = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(p0.x, Mathf.Min(p1.x, p2.x))), 0, texWidth - 1);
        int maxX = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(p0.x, Mathf.Max(p1.x, p2.x))), 0, texWidth - 1);
        int minY = Mathf.Clamp(Mathf.FloorToInt(Mathf.Min(p0.y, Mathf.Min(p1.y, p2.y))), 0, texHeight - 1);
        int maxY = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(p0.y, Mathf.Max(p1.y, p2.y))), 0, texHeight - 1);

        Vector2 v0 = p1 - p0;
        Vector2 v1 = p2 - p0;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);
                Vector2 v2 = p - p0;

                float dot00 = Vector2.Dot(v0, v0);
                float dot01 = Vector2.Dot(v0, v1);
                float dot02 = Vector2.Dot(v0, v2);
                float dot11 = Vector2.Dot(v1, v1);
                float dot12 = Vector2.Dot(v1, v2);

                float denom = dot00 * dot11 - dot01 * dot01;
                if (Mathf.Approximately(denom, 0f)) continue;

                float invDenom = 1f / denom;
                float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
                float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

                if (u >= 0 && v >= 0 && u + v <= 1)
                {
                    coveredPixels.Add(new Vector2Int(x, y));
                }
            }
        }
    }
}