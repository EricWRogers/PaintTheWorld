using UnityEngine;

public class SprayPaintLine : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int currentAmmo = 100;
    public int maxAmmo = 200;
    public float consumptionRate = 10f;

    [Header("Effects & References")]
    public ParticleSystem sprayParticles;
    public Transform nozzleSpawnPoint; // Also serves as the projectile spawn point
    public Transform playerCamera;

    [Header("Aiming Settings")]
    public float maxAimDistance = 20f;
    public float rotationSmoothing = 20f;
    public LayerMask playerMask;

    [Header("Projectile Settings")]
    public GameObject projectilePrefab;
    public float launchForce = 800f;
    public float projectileLifetime = 5f;
    public float projectileInterval = 2f; // Seconds between spawns while spraying

    private float ammoRemainder = 0f;
    private bool isSpraying = false;
    private float projectileTimer = 0f;

    [Header("Paint Settings")]
    public int canColorKey = 0; // The ID for this can's color (e.g., 0 for Red, 1 for Blue)
    private ParticlePainter2 painter;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main.transform;

        if (sprayParticles != null)
        {
            var main = sprayParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            if (sprayParticles.isPlaying) sprayParticles.Stop();

            painter = GetComponentInChildren<ParticlePainter2>();

            UpdatePainterColor();
        }
    }

    private void Update()
    {
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
        // Hold left mouse to spray and start the projectile timer
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
            
            ShootProjectile(); 
        }
    }

    private void StopSpraying()
    {
        if (isSpraying)
        {
            isSpraying = false;
            if (sprayParticles != null && sprayParticles.isPlaying) sprayParticles.Stop();
            projectileTimer = 0f; // Reset timer when stopping
        }
    }

    private void HandleProjectileSpawning()
    {
        projectileTimer += Time.deltaTime;

        if (projectileTimer >= projectileInterval)
        {
            ShootProjectile();
            projectileTimer = 0f;
        }
    }

    private void ShootProjectile()
    {
        if (projectilePrefab == null || nozzleSpawnPoint == null) return;

        // Create the projectile at the nozzle position and rotation
        GameObject proj = Instantiate(projectilePrefab, nozzleSpawnPoint.position, nozzleSpawnPoint.rotation);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null) rb = proj.AddComponent<Rigidbody>();

        rb.AddForce(nozzleSpawnPoint.forward * launchForce);

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
        painter.UpdateColorFromManager(); // We will create this method next
    }
}
    // UI and External Refill Support
    public void AddAmmo(int amount) => currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    public float GetAmmoPercentage() => maxAmmo <= 0 ? 0f : ((float)currentAmmo / maxAmmo) * 100f;
    public float GetNormalizedAmmo() => maxAmmo <= 0 ? 0f : (float)currentAmmo / (float)maxAmmo;
}