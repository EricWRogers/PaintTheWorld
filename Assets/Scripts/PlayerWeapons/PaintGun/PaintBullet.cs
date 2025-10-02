using UnityEngine;
using SuperPupSystems.Helper;

public class PaintBullet : RayCastPainter
{
    private Bullet m_bullet;
    void Start()
    {
        m_bullet = GetComponent<Bullet>();
    }
    void Update()
    {
    }
    public void paint()
    {
        TryPaint(m_bullet.hitInfo);
    }
}
