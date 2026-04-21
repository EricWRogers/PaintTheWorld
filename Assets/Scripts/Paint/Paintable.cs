    using UnityEngine;





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
    

    void Awake() {
        m_maskRenderTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        m_maskRenderTexture.filterMode = FilterMode.Bilinear;
        m_maskRenderTexture.useMipMap = true;
        m_maskRenderTexture.autoGenerateMips = false;
        m_maskRenderTexture.enableRandomWrite = false;
        m_maskRenderTexture.Create();

        m_supportTexture = new RenderTexture((int)TEXTURE_SIZE, (int)TEXTURE_SIZE, 0);
        m_supportTexture.filterMode = FilterMode.Bilinear;

        m_rend = GetComponent<Renderer>();
        m_rend.material.SetTexture(maskTextureID, m_maskRenderTexture);
    }
    void Start()
    {
        PaintManager.instance.initTextures(this);
    }
    void Update()
    {
        
    }

    public void ResetPaint()
    {
        RenderTexture activeRend = RenderTexture.active;
        RenderTexture.active = m_maskRenderTexture;

        GL.Clear(true, true, Color.clear);

        RenderTexture.active = activeRend;

        if (m_maskRenderTexture != null && m_maskRenderTexture.IsCreated())
        {
            m_maskRenderTexture.GenerateMips();
        }

        if (m_supportTexture != null)
        {
            RenderTexture.active = m_supportTexture;
            GL.Clear(true, true, Color.clear);
            RenderTexture.active = activeRend;
        }
    }

    void OnEnable()
    {
        if (m_maskRenderTexture == null) return;
            ResetPaint();
    }
    void OnDisable(){
        if(m_maskRenderTexture != null)
            m_maskRenderTexture.Release();
        
        if(m_supportTexture != null)
            m_supportTexture.Release();
    }

    
}