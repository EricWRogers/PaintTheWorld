using UnityEngine;
using SuperPupSystems.Helper;

public class PaintBullet : RayCastPainter
{
    private Bullet m_bullet;
    void Start()
    {
        m_bullet = GetComponent<Bullet>();
        Renderer blobRenderer = gameObject.GetComponent<Renderer>();
        blobRenderer.material.SetColor("_BaseColor", paintColor);
    }
    void Update()
    {
    }
    public void paint()
    {
        TryPaint(m_bullet.hitInfo);
    }
}
