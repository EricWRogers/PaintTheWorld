using UnityEngine;
using UnityEngine.Rendering;

public class Paintable : MonoBehaviour {

    public enum TextureQuality
    {
        BIG = 1024,
        MEDIUM = 512,
        SMALL = 256,
        MINI = 124,
    }
    //const int TEXTURE_SIZE = 256;
    public TextureQuality TEXTURE_SIZE = TextureQuality.MINI;
    public float extendsIslandOffset = 1;

    private RenderTexture m_extendIslandsRenderTexture;
    private RenderTexture m_uvIslandsRenderTexture;
    private RenderTexture m_maskRenderTexture;
    private RenderTexture m_supportTexture;
    
    private Renderer m_rend;

    private int m_maskTextureID = Shader.PropertyToID("_MaskTexture");

    public RenderTexture getMask() => m_maskRenderTexture;
    public RenderTexture getUVIslands() => m_uvIslandsRenderTexture;
    public RenderTexture getExtend() => m_extendIslandsRenderTexture;
    public RenderTexture getSupport() => m_supportTexture;
    public Renderer getRenderer() => m_rend;

    void Start() {
        m_maskRenderTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        m_maskRenderTexture.filterMode = FilterMode.Bilinear;

        m_extendIslandsRenderTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        m_extendIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        // m_uvIslandsRenderTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        // m_uvIslandsRenderTexture.filterMode = FilterMode.Bilinear;

        m_supportTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        m_supportTexture.filterMode =  FilterMode.Bilinear;

        m_rend = GetComponent<Renderer>();
        m_rend.material.SetTexture(m_maskTextureID, m_extendIslandsRenderTexture);

        PaintManager.instance.initTextures(this);
    }

    void OnDisable(){
        m_maskRenderTexture.Release();
        //m_uvIslandsRenderTexture.Release();
        m_extendIslandsRenderTexture.Release();
        m_supportTexture.Release();
    }
}