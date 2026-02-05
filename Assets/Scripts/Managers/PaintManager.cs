using UnityEngine;
using UnityEngine.Rendering;

public class PaintManager : Singleton<PaintManager>
{

    public Shader texturePaint;

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

    private CommandBuffer m_command;

    protected override void Awake()
    {
        base.Awake();
        
        m_paintMaterial = new Material(texturePaint);
        m_command = new CommandBuffer();
        m_command.name = "CommmandBuffer - " + gameObject.name;
    }

    public void initTextures(Paintable paintable)
    {
        RenderTexture mask = paintable.getMask();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        m_command.SetRenderTarget(mask);
        m_command.SetRenderTarget(support);

        m_paintMaterial.SetFloat(m_prepareUVID, 1);
        m_command.DrawRenderer(rend, m_paintMaterial, 0);

        Graphics.ExecuteCommandBuffer(m_command);
        m_command.Clear();
    }


    public void paint(Paintable paintable, Vector3 pos, float radius = 1f, float hardness = .5f, float strength = .5f, Color? color = null)
    {
        RenderTexture mask = paintable.getMask();
        RenderTexture support = paintable.getSupport();
        Renderer rend = paintable.getRenderer();

        m_paintMaterial.SetFloat(m_prepareUVID, 0);
        m_paintMaterial.SetVector(m_positionID, pos);
        m_paintMaterial.SetFloat(m_hardnessID, hardness);
        m_paintMaterial.SetFloat(m_strengthID, strength);
        m_paintMaterial.SetFloat(m_radiusID, radius);
        m_paintMaterial.SetTexture(m_textureID, support);    // previous content
        m_paintMaterial.SetColor(m_colorID, color ?? Color.red);

        // 1) Draw onto mask
        m_command.SetRenderTarget(mask);
        //m_command.ClearRenderTarget(true, true, color ?? Color.red); // optional clear
        m_command.DrawRenderer(rend, m_paintMaterial, 0);

        // 2) Copy/accumulate mask into support
        m_command.SetRenderTarget(support);
        m_command.Blit(mask, support);

        Graphics.ExecuteCommandBuffer(m_command);
        m_command.Clear();

        float paintedAmount = Mathf.Max(0.01f, radius * radius * strength);
        GameEvents.PaintApplied?.Invoke(paintedAmount);
        mask.GenerateMips();
        paintable.percentageCovered = paintable.GetPaintCoverage(mask);
    }

}
