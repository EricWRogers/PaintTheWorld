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
        m_bullet.hitTarget.AddListener(OnHitEffects);
    }
    public void paint()
    {
        TryPaint(m_bullet.hitInfo);
    }
    public void OnHitEffects()
    {
        GameEvents.PlayerHitEnemy.Invoke(m_bullet.hitInfo.transform.gameObject, m_bullet.damage, HitSource.PlayerWeapon);
    }
}
