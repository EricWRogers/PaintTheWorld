using System.Collections;
using UnityEngine;

public class SprayPaintLine : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int currentAmmo = 100;
    public int baseMaxAmmo = 200;
    public int maxAmmo = 200;
    public float consumptionRate = 10f;

    [Header("Runtime Bonuses")]
    public int bonusMaxAmmo = 0;
    public int bonusProjectileCount = 0;

    [Header("Effects & References")]
    public ParticleSystem sprayParticles;
    public Transform nozzleSpawnPoint;
    public Transform playerCamera;

    [Header("Aiming Settings")]
    public float maxAimDistance = 20f;
    public float rotationSmoothing = 20f;
    public LayerMask playerMask;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float launchForce = 800f;
    public float projectileLifetime = 5f;
    public float projectileInterval = 2f;

    [Header("Extra Projectile Timing")]
    public float extraProjectileDelay = 0.5f;

    private float ammoRemainder = 0f;
    private bool isSpraying = false;
    private float projectileTimer = 0f;

    [Header("Paint Settings")]
    public int canColorKey = 0;
    private ParticlePainter painter;

    public int ProjectileCount => Mathf.Max(1, 1 + bonusProjectileCount);

    private void Start()
    {
        ApplyRuntimeStats();

        if (playerCamera == null) playerCamera = Camera.main.transform;

        if (sprayParticles != null)
        {
            var main = sprayParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            if (sprayParticles.isPlaying) sprayParticles.Stop();

            painter = GetComponentInChildren<ParticlePainter>();
            UpdatePainterColor();
        }
    }

    private void Update()
    {
        ApplyRuntimeStats();
        HandleInput();

        if (isSpraying)
        {
            HandleProjectileSpawning();
        }
    }

    private void LateUpdate()
    {
        UpdateAimingAndPosition();
    }

    public void ApplyRuntimeStats()
    {
        maxAmmo = baseMaxAmmo + bonusMaxAmmo;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
    }

    public void SetBonusMaxAmmo(int amount, bool alsoFillAddedCapacity = true)
    {
        int oldMax = maxAmmo;
        bonusMaxAmmo = Mathf.Max(0, amount);
        ApplyRuntimeStats();

        if (alsoFillAddedCapacity && maxAmmo > oldMax)
        {
            currentAmmo = Mathf.Min(maxAmmo, currentAmmo + (maxAmmo - oldMax));
        }
    }

    public void SetBonusProjectileCount(int amount)
    {
        bonusProjectileCount = Mathf.Max(0, amount);
    }

    private void UpdateAimingAndPosition()
    {
        if (playerCamera == null || nozzleSpawnPoint == null || sprayParticles == null) return;

        sprayParticles.transform.position = nozzleSpawnPoint.position;

        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, ~playerMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            targetPoint = ray.GetPoint(10f);
        }

        Vector3 direction = (targetPoint - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
            sprayParticles.transform.rotation = transform.rotation;
        }
    }

    private void HandleInput()
    {
        if (PlayerManager.instance.playerInputs.Attack.IsPressed() && currentAmmo > 0)
        {
            StartSpraying();
            ConsumeAmmo();
        }
        else
        {
            StopSpraying();
        }
    }

    private void StartSpraying()
    {
        if (!isSpraying)
        {
            isSpraying = true;
            if (sprayParticles != null && !sprayParticles.isPlaying) sprayParticles.Play();

            ShootProjectileBurst();
        }
    }

    private void StopSpraying()
    {
        if (isSpraying)
        {
            isSpraying = false;
            if (sprayParticles != null && sprayParticles.isPlaying) sprayParticles.Stop();
            projectileTimer = 0f;
        }
    }

    private void HandleProjectileSpawning()
    {
        projectileTimer += Time.deltaTime;

        if (projectileTimer >= projectileInterval)
        {
            ShootProjectileBurst();
            projectileTimer = 0f;
        }
    }

    private void ShootProjectileBurst()
    {
        if (projectilePrefab == null || nozzleSpawnPoint == null) return;

        
        Vector3 spawnPosition = nozzleSpawnPoint.position;
        Quaternion spawnRotation = nozzleSpawnPoint.rotation;
        Vector3 launchDirection = nozzleSpawnPoint.forward;

        
        SpawnProjectileOriginal(spawnPosition, spawnRotation, launchDirection);

        
        int extraShots = ProjectileCount - 1;
        if (extraShots > 0)
        {
            StartCoroutine(FireExtraProjectiles(extraShots, spawnPosition, spawnRotation, launchDirection));
        }
    }

    private IEnumerator FireExtraProjectiles(int extraShots, Vector3 spawnPosition, Quaternion spawnRotation, Vector3 launchDirection)
    {
        for (int i = 0; i < extraShots; i++)
        {
            yield return new WaitForSeconds(extraProjectileDelay);
            SpawnProjectileOriginal(spawnPosition, spawnRotation, launchDirection);
        }
    }

    private void SpawnProjectileOriginal(Vector3 spawnPosition, Quaternion spawnRotation, Vector3 launchDirection)
    {
        
        GameObject proj = Instantiate(projectilePrefab, spawnPosition, spawnRotation);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) rb = proj.AddComponent<Rigidbody>();

        rb.AddForce(launchDirection * launchForce);

        Destroy(proj, projectileLifetime);
    }

    private void ConsumeAmmo()
    {
        ammoRemainder += consumptionRate * Time.deltaTime;
        if (ammoRemainder >= 1f)
        {
            int ammoToSubtract = Mathf.FloorToInt(ammoRemainder);
            currentAmmo = Mathf.Max(0, currentAmmo - ammoToSubtract);
            ammoRemainder -= ammoToSubtract;
        }
    }

    public void UpdatePainterColor()
    {
        if (painter != null)
        {
            painter.colorKey = canColorKey;
            painter.UpdateColorFromManager();
        }
    }

    public void AddAmmo(int amount) => currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    public float GetAmmoPercentage() => maxAmmo <= 0 ? 0f : ((float)currentAmmo / maxAmmo) * 100f;
    public float GetNormalizedAmmo() => maxAmmo <= 0 ? 0f : (float)currentAmmo / (float)maxAmmo;
}