using UnityEngine;
using SuperPupSystems.Helper;

public class PaintLauncher : Weapon
{
    [Header("Launcher Settings")]
    public GameObject bulletPrefab; 
    public float fireRate = 0.5f;   
    public float spreadAngle = 6f;

    private float m_nextFireTime = 0f; 
    private PlayerPaint m_playerPaint; 

    new void Start()
    {
        base.Start();
        if (player != null)
        {
            m_playerPaint = player.GetComponent<PlayerPaint>();
        }
    }

    new void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            if (Time.time >= m_nextFireTime)
            {
                Fire();
                
                m_nextFireTime = Time.time + fireRate;
            }
        }
    }
    public override void Fire()
    {
        Shoot();
    }

    // Instantiates and fires a single projectile forward.
    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Quaternion finalRotation = firePoint.rotation;

        // Apply spread
        float halfAngle = spreadAngle * 0.5f;
        float yaw = Random.Range(-halfAngle, halfAngle);
        float pitch = Random.Range(-halfAngle, halfAngle);
        Quaternion spread = Quaternion.Euler(pitch, yaw, 0f);

        finalRotation *= spread;

        // Create the bullet
        GameObject temp = Instantiate(bulletPrefab, firePoint.position, finalRotation);
        
        // Set paint color
        if (m_playerPaint != null)
        {
            var pb = temp.GetComponent<PaintBullet>();
            if (pb != null) pb.paintColor = m_playerPaint.selectedPaint;
        }

        // Set damage
        var bullet = temp.GetComponent<Bullet>();
        if (bullet != null)
            bullet.damage = Mathf.RoundToInt(damage * damageMult);
    }
}