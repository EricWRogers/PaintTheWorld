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
    public float rotationSpeed = 10f; 

    public GameObject bulletPrefab;

    private bool m_isFiringBurst = false;
    private int m_bulletsFiredThisBurst = 0;
    private float m_shotTimer = 0f;
    private float m_autoFireTimer;
    private float m_nextBurstAvailableTime = 0f;
    private Transform m_currentTarget;
    private PlayerPaint m_playerPaint; 

    private float shotInterval => 60f / Mathf.Max(1f, roundsPerMinute * attackSpeedMult);

    // Add an Awake method to cache the component
    void Awake()
    {

    }

    // Use the base class Start() to get the player
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
        if (m_currentTarget == null || !m_currentTarget.gameObject.activeInHierarchy)
        {
            FindClosestTarget();
        }

        if (m_currentTarget != null)
        {
            // Check if target is still in range
            if (Vector3.Distance(player.transform.position, m_currentTarget.position) > targetRadius)
            {
                m_currentTarget = null;
            }
            else
            {               
                // Get the direction from the gun's pivot to the target's pivot.
                Vector3 directionToTarget = m_currentTarget.position - transform.position;

                // Avoid error if target is at the exact same position
                if (directionToTarget != Vector3.zero) 
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
                    // Apply the rotation to 'transform' (the PaintGun object)
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
                }
            }
        }
        else
        {
            // No target, so return to forward position.
            if(player != null)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, player.transform.rotation, rotationSpeed * Time.deltaTime);
            }
        }

        m_autoFireTimer -= Time.deltaTime;

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
        
        if (autoFire)
        {
            if (m_currentTarget != null && m_autoFireTimer < 0 && !m_isFiringBurst && Time.time >= m_nextBurstAvailableTime)
            {
                StartBurst();
                m_autoFireTimer = autoFireRate;
            }
        }
    }

    private void FindClosestTarget()
    {
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

        m_currentTarget = closestEnemy; 
    }

    private void StartBurst()
    {

        if (m_currentTarget == null)
        {
            FindClosestTarget(); 
            if (m_currentTarget == null) return;
        }
        
        m_isFiringBurst = true;
        m_bulletsFiredThisBurst = 0;
        m_shotTimer = 0f;
        Shoot(); // Fire the first bullet immediately
        m_bulletsFiredThisBurst++;
    }
    private void Shoot()
    {
        if (bulletPrefab == null || firePoint == null) return;

        Quaternion finalRotation = firePoint.rotation;

        float halfAngle = spreadAngle * 0.5f;
        float yaw = Random.Range(-halfAngle, halfAngle);
        float pitch = Random.Range(-halfAngle, halfAngle);
        Quaternion spread = Quaternion.Euler(pitch, yaw, 0f);

        finalRotation *= spread;

        GameObject temp = Instantiate(bulletPrefab, firePoint.position, finalRotation);
        
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
        // m_currentTarget = null;
    }
}