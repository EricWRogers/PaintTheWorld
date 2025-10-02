using UnityEngine;
using SuperPupSystems.Helper;

public class PaintBallGun : Weapon
{
    [Header("Burst Settings")]
    public int bulletsPerBurst = 3;
    public float roundsPerMinute = 600f;
    public float timeBetweenBursts = 0.25f;
    public float spreadAngle = 6f;

    public GameObject bulletPrefab;

    private bool m_isFiringBurst = false;
    private int m_bulletsFiredThisBurst = 0;
    private float m_shotTimer = 0f;
    private float m_nextBurstAvailableTime = 0f;

    private float shotInterval => 60f / Mathf.Max(1f, roundsPerMinute * attackSpeedMult);

    void Update()
    {
        if (Input.GetButton("Fire1") && !m_isFiringBurst && Time.time >= m_nextBurstAvailableTime)
        {
            StartBurst();
        }

        if (m_isFiringBurst)
        {
            m_shotTimer += Time.deltaTime;

            if (m_shotTimer >= shotInterval)
            {
                m_shotTimer -= shotInterval;
                Shoot();
                m_bulletsFiredThisBurst++;

                if (m_bulletsFiredThisBurst >= bulletsPerBurst)
                {
                    m_isFiringBurst = false;
                    m_nextBurstAvailableTime = Time.time + (timeBetweenBursts / Mathf.Max(0.0001f, attackSpeedMult));
                }
            }
        }
    }

    private void StartBurst()
    {
        m_isFiringBurst = true;
        m_bulletsFiredThisBurst = 0;
        m_shotTimer = 0f;

        Shoot();
        m_bulletsFiredThisBurst++;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        float halfAngle = spreadAngle * 0.5f;
        float yaw = Random.Range(-halfAngle, halfAngle);
        float pitch = Random.Range(-halfAngle, halfAngle);

        Quaternion spreadRot = Quaternion.Euler(pitch, yaw, 0f) * firePoint.rotation;
        GameObject temp = Instantiate(bulletPrefab, firePoint.position, spreadRot);
        temp.GetComponent<PaintBullet>().paintColor = player.GetComponent<PlayerPaint>().selectedPaint;
        Bullet bullet = temp.GetComponent<Bullet>();
        if (bullet != null)
            bullet.damage = Mathf.RoundToInt(damage * damageMult);
    }

    public override void Fire()
    {
    }

    public void CancelBurst()
    {
        m_isFiringBurst = false;
        m_bulletsFiredThisBurst = 0;
        m_shotTimer = 0f;
    }
}
