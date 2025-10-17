using UnityEngine;
using SuperPupSystems.Helper;
using System.Collections.Generic;

public class ItemBullet : RayCastPainter
{
    private Bullet m_bullet;
    public LayerMask layerMask;
    void Start()
    {
        m_bullet = GetComponent<Bullet>();
        Renderer blobRenderer = gameObject.GetComponent<Renderer>();
        blobRenderer.material.SetColor("_BaseColor", paintColor);
        List<GameObject> closeEnemies = EnemyFinder.FindClosest(PlayerManager.instance.player.transform.position, 20, layerMask);
        transform.LookAt(closeEnemies[0].transform);
    }
    void Update()
    {
    }
    public void paint()
    {
        TryPaint(m_bullet.hitInfo);
    }
}


