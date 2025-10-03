using UnityEngine;
using UnityEngine.Rendering;

public class PaintManager : Singleton<PaintManager>
{

    public Shader texturePaint;
    public Shader extendIslands;

    private int m_prepareUVID = Shader.PropertyToID("_PrepareUV");
    private int m_positionID = Shader.PropertyToID("_PainterPosition");
    private int m_hardnessID = Shader.PropertyToID("_Hardness");
    private int m_strengthID = Shader.PropertyToID("_Strength");
    private int m_radiusID = Shader.PropertyToID("_Radius");
    private int m_blendOpID = Shader.PropertyToID("_BlendOp");
    private int m_colorID = Shader.PropertyToID("_PainterColor");
    private int m_textureID = Shader.PropertyToID("_MainTex");
    private int m_uvOffsetID = Shader.PropertyToID("_OffsetUV");
    private int m_uvIslandsID = Shader.PropertyToID("_UVIslands");

    private Material m_paintMaterial;
    private Material m_extendMaterial;

    private CommandBuffer m_command;

    public override void Awake()
    {
        base.Awake();
        
        m_paintMaterial = new Material(texturePaint);
        m_extendMaterial = new Material(extendIslands);
        m_command = new CommandBuffer();
        m_command.name = "CommmandBuffer - " + gameObject.name;
    }

    public void initTextures(Paintable paintable)
    {
        RenderTexture mask = paintable.getMask();
        RenderTexture uvIslands = paintable.getUVIslands();
        RenderTexture extend = paintable.getExtend();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        m_command.SetRenderTarget(mask);
        m_command.SetRenderTarget(extend);
        m_command.SetRenderTarget(support);

        m_paintMaterial.SetFloat(m_prepareUVID, 1);
        m_command.SetRenderTarget(uvIslands);
        m_command.DrawRenderer(rend, m_paintMaterial, 0);

        Graphics.ExecuteCommandBuffer(m_command);
        m_command.Clear();
    }


    public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)
    {
        RenderTexture mask = paintable.getMask();
        RenderTexture uvIslands = paintable.getUVIslands();
        RenderTexture extend = paintable.getExtend();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        m_paintMaterial.SetFloat(m_prepareUVID, 0);
        m_paintMaterial.SetVector(m_positionID, pos);
        m_paintMaterial.SetFloat(m_hardnessID, hardness);
        m_paintMaterial.SetFloat(m_strengthID, strength);
        m_paintMaterial.SetFloat(m_radiusID, radius);
        m_paintMaterial.SetTexture(m_textureID, support);
        m_paintMaterial.SetColor(m_colorID, color ?? Color.red);
        m_extendMaterial.SetFloat(m_uvOffsetID, paintable.extendsIslandOffset);
        m_extendMaterial.SetTexture(m_uvIslandsID, uvIslands);

        m_command.SetRenderTarget(mask);
        m_command.DrawRenderer(rend, m_paintMaterial, 0);

        m_command.SetRenderTarget(support);
        m_command.Blit(mask, support);

        m_command.SetRenderTarget(extend);
        m_command.Blit(mask, extend, m_extendMaterial);

        Graphics.ExecuteCommandBuffer(m_command);
        m_command.Clear();
    }

}
