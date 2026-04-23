using UnityEngine;
using System.Collections.Generic;
using Unity.VisualScripting;


#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(PaintingObj))]
public class PaintableEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        PaintingObj paintingObj = (PaintingObj)target;

        if (GUILayout.Button("Calculate Material Percentage"))
        {
            paintingObj.CalculateTextureCoverage();
        }

    }
}
#endif

public class PaintingObj : MonoBehaviour
{
    public Paintable paintable;
    public int coinsGainedWhileHolding;
    public int coinsGainedOnCapture;
    public float coinGainDelay = 3;
    public float scoreGainedOnHold;
    private float m_timer;
    public bool covered;
    public float targetCoverPercent;
    public float percentageCovered;
    // public float meshPercent;
    private float checkTimer = 0f;
    public float checkInterval = 0.5f;
    public int currentEnemiesTarget;
    public Transform playerSpawnPoint;
    private bool hasCaptured = false;
    public List<Texture> posters;


    void Awake()
    {
        if(paintable != null)
            paintable = GetComponent<Paintable>();
        
        GetComponent<Renderer>().material.SetTexture("_MainTex", posters[Random.Range(0, posters.Count-1)]);
    }
    void Start()
    {
        percentageCovered = 0;
    }

    // Update is called once per frame
    void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer >= checkInterval)
        {
            // Update the actual percentage from the RenderTexture
            percentageCovered = GetPaintCoverage(paintable.getMask());
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

        if(covered && !hasCaptured)
        {
            if(GameManager.instance.currentGamemode == GameManager.gameModes.HoldPoints)
            {
                m_timer -= Time.deltaTime;
                if(m_timer <= 0)
                {
                    PlayerManager.instance.wallet.Add(coinsGainedWhileHolding);
                    GameManager.instance.timeHeld += scoreGainedOnHold;
                    m_timer = coinGainDelay;
                }
            }
            else if(GameManager.instance.currentGamemode == GameManager.gameModes.CapturePoints)
            {
                hasCaptured = true;
                PlayerManager.instance.wallet.Add(coinsGainedOnCapture);
                GameManager.instance.amountCaptured++;
                percentageCovered = 0;
                GameManager.instance.RemoveObjective(this);
            }
        }
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

        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.ReadPixels(new Rect(0, 0, 1, 1), 0, 0);
        tex.Apply();

        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(temp);
        Color pixel = tex.GetPixel(0, 0);
        float coverage = Mathf.Max(pixel.r, pixel.g, pixel.b);
        return coverage * 100f;
    }

    public void CalculateTextureCoverage()
    {
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        Mesh mesh = meshFilter.sharedMesh;

        Vector2[] uvs = mesh.uv;

        int texWidth = (int)paintable.TEXTURE_SIZE;
        int texHeight = (int)paintable.TEXTURE_SIZE;
        int totalPixels = texWidth * texHeight;

        HashSet<Vector2Int> coveredPixels = new HashSet<Vector2Int>();

        int[] triangles = mesh.triangles;

        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector2 uv1 = uvs[triangles[i]] * (int)paintable.TEXTURE_SIZE;
            Vector2 uv2 = uvs[triangles[i + 1]] * (int)paintable.TEXTURE_SIZE;
            Vector2 uv3 = uvs[triangles[i + 2]] * (int)paintable.TEXTURE_SIZE;

            RasterizeTriangle(uv1, uv2, uv3, coveredPixels, texWidth, texHeight);
        }

        //meshPercent = 100 - ((float)coveredPixels.Count / totalPixels * 100f);
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
    void OnEnable()
    {
        if(paintable == null)
            paintable = GetComponent<Paintable>();

        if(paintable != null)
            paintable.ResetPaint();

        percentageCovered = 0;
        covered = false;
        currentEnemiesTarget = 0;
        hasCaptured = false;
    }
}
