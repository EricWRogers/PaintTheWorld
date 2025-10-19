using UnityEngine;
using SuperPupSystems.Helper;

public class PaintBallGun : Weapon
{
    [Header("Burst Settings")]
    public int bulletsPerBurst = 3;
    public float roundsPerMinute = 600f;
    public float timeBetweenBursts = 0.25f;
    public float spreadAngle = 6f;
    public bool autoFire;
    public float autoFireRate;

    [Header("Targeting Settings")]
    public float targetRadius = 15f;
    public string enemyTag = "Enemy";
    public float rotationSpeed = 10f; // Controls how fast the player turns

    public GameObject bulletPrefab;

    private bool m_isFiringBurst = false;
    private int m_bulletsFiredThisBurst = 0;
    private float m_shotTimer = 0f;
    private float m_autoFireTimer;
    private float m_nextBurstAvailableTime = 0f;
    private Transform m_currentTarget; // Keep track of the current target
    private PlayerPaint m_playerPaint; // Variable to hold the PlayerPaint component

    private float shotInterval => 60f / Mathf.Max(1f, roundsPerMinute * attackSpeedMult);

    // Add an Awake method to cache the component
    void Awake()
    {
        // Cache the PlayerPaint component from the player GameObject once
        if (player != null)
        {
            m_playerPaint = player.GetComponent<PlayerPaint>();
        }
    }

    new void Update()
    {
        base.Update();
        // Smoothly rotate the player towards the current target every frame
        if (m_currentTarget != null)
        {
            Vector3 lookAtPosition = new Vector3(m_currentTarget.position.x, player.transform.position.y, m_currentTarget.position.z);
            Quaternion targetRotation = Quaternion.LookRotation(lookAtPosition - player.transform.position);
            player.transform.rotation = Quaternion.Slerp(player.transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        // Check for firing input
        // if (Input.GetButton("Fire1") && !m_isFiringBurst && Time.time >= m_nextBurstAvailableTime)
        // {
        //     StartBurst();
        // }
        m_autoFireTimer -= Time.deltaTime;
        // Handle the burst firing logic
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
                    m_currentTarget = null; // Clear the target after the burst is complete
                }
            }
        }
        if (autoFire)
        {
            if (m_autoFireTimer < 0 && !m_isFiringBurst && Time.time >= m_nextBurstAvailableTime)
            {
                StartBurst();
                m_autoFireTimer = autoFireRate;
            }
        }
    }

    private void StartBurst()
    {
        m_isFiringBurst = true;
        m_bulletsFiredThisBurst = 0;
        m_shotTimer = 0f;
        Shoot(); // Fire the first bullet immediately
        m_bulletsFiredThisBurst++;
    }

    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        // Find the closest enemy
        Collider[] hitColliders = Physics.OverlapSphere(firePoint.position, targetRadius);
        Transform closestEnemy = null;
        float minDistance = float.MaxValue;

        foreach (var hitCollider in hitColliders)
        {
            if (hitCollider.CompareTag(enemyTag))
            {
                float distance = Vector3.Distance(firePoint.position, hitCollider.transform.position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEnemy = hitCollider.transform;
                }
            }
        }

        Quaternion finalRotation;

        // If an enemy was found, set it as the current target and aim the projectile
        if (closestEnemy != null)
        {
            m_currentTarget = closestEnemy;
            Vector3 directionToEnemy = (closestEnemy.position - firePoint.position).normalized;
            finalRotation = Quaternion.LookRotation(directionToEnemy);
        }
        else
        {
            m_currentTarget = null; // Ensure no target if none is in range
            finalRotation = firePoint.rotation;
        }

        // Apply random spread to the final rotation
        float halfAngle = spreadAngle * 0.5f;
        float yaw = Random.Range(-halfAngle, halfAngle);
        float pitch = Random.Range(-halfAngle, halfAngle);
        Quaternion spread = Quaternion.Euler(pitch, yaw, 0f);

        finalRotation *= spread;

        // Instantiate and configure the bullet
        GameObject temp = Instantiate(bulletPrefab, firePoint.position, finalRotation);
        
        // Use the cached component reference here
        if (m_playerPaint != null)
        {
            temp.GetComponent<PaintBullet>().paintColor = m_playerPaint.selectedPaint;
        }

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
        m_currentTarget = null;
    }
}