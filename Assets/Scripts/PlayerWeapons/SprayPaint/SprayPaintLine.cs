using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

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
    public float bonusAttackSpeedMultiplier = 1f;

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
    public float projectileInterval = 0.5f;

    [Header("Extra Projectile Timing")]
    public float extraProjectileDelay = 0.4f;

    [Header("Cooldown Settings")]
    public float attackCooldown = 0.5f;

    [Header("Paint Settings")]
    public int canColorKey = 0;

    [Header("Animation Settings")]
    public Animator weaponAnimator;
    public string attackTriggerName = "AttackTrigger";

    [Header("Crosshair UI")]
    public Image crosshairImage;
    public Color normalCrosshairColor = Color.white;
    public Color enemyCrosshairColor = Color.red;

    private float ammoRemainder = 0f;
    private bool isSpraying = false;
    private float projectileTimer = 0f;
    private float lastAttackTime = -999f;
    private bool canCombo = false;

    private ParticlePainter painter;

    public int ProjectileCount => Mathf.Max(1, 1 + bonusProjectileCount);

    public float CurrentProjectileInterval
    {
        get
        {
            float mult = Mathf.Max(0.1f, bonusAttackSpeedMultiplier);
            return projectileInterval / mult;
        }
    }

    private void Start()
    {
        ApplyRuntimeStats();

        if (playerCamera == null && Camera.main != null)
            playerCamera = Camera.main.transform;

        TryAutoAssignCrosshair();

        if (sprayParticles != null)
        {
            var main = sprayParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;

            if (sprayParticles.isPlaying)
                sprayParticles.Stop();

            painter = GetComponentInChildren<ParticlePainter>();
            UpdatePainterColor();
        }

        SetCrosshairColor(normalCrosshairColor);
    }

    private void Update()
    {
        ApplyRuntimeStats();

        if (weaponAnimator != null)
        {
            var stateInfo = weaponAnimator.GetCurrentAnimatorStateInfo(1);
            bool isAttacking =
                stateInfo.IsTag("Attack") ||
                stateInfo.IsName("metarig|ATTACK_V2") ||
                stateInfo.IsName("metarig|ATTACK_V2 0");

            float targetWeight = isAttacking ? 1f : 0f;
            float currentWeight = weaponAnimator.GetLayerWeight(1);
            float lerpedWeight = Mathf.Lerp(currentWeight, targetWeight, Time.deltaTime * 15f);
            weaponAnimator.SetLayerWeight(1, lerpedWeight);
        }

        HandleInput();

        if (isSpraying && currentAmmo > 0)
        {
            ConsumeAmmo();
            HandleProjectileSpawning();
        }
    }

    private void LateUpdate()
    {
        UpdateAimingAndCrosshair();
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

    private void TryAutoAssignCrosshair()
    {
        if (crosshairImage != null)
            return;

        CrosshairReference crosshairRef = FindObjectOfType<CrosshairReference>(true);
        if (crosshairRef != null && crosshairRef.image != null)
        {
            crosshairImage = crosshairRef.image;
            Debug.Log("Auto-assigned crosshair image: " + crosshairImage.name);
            return;
        }

        Debug.LogWarning("Crosshair image could not be auto-assigned");
    }

    public void SetBonusProjectileCount(int amount)
    {
        bonusProjectileCount = Mathf.Max(0, amount);
    }

    public void SetAttackSpeedMultiplier(float multiplier)
    {
        bonusAttackSpeedMultiplier = Mathf.Max(1f, multiplier);
    }

    private void UpdateAimingAndCrosshair()
    {
        if (playerCamera == null || nozzleSpawnPoint == null)
            return;

        if (sprayParticles != null)
            sprayParticles.transform.position = nozzleSpawnPoint.position;

        Vector3 rayOrigin = playerCamera.position + (playerCamera.forward * 0.5f);
        Ray ray = new Ray(rayOrigin, playerCamera.forward);

        Vector3 targetPoint;
        bool aimingAtEnemy = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, ~playerMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;

            if (hit.collider.CompareTag("Enemy"))
                aimingAtEnemy = true;
        }
        else
        {
            targetPoint = ray.GetPoint(maxAimDistance);
        }

        Vector3 direction = (targetPoint - nozzleSpawnPoint.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            nozzleSpawnPoint.rotation = Quaternion.Slerp(
                nozzleSpawnPoint.rotation,
                targetRotation,
                Time.deltaTime * rotationSmoothing
            );

            if (sprayParticles != null)
            {
                sprayParticles.transform.position = nozzleSpawnPoint.position;
                sprayParticles.transform.rotation = nozzleSpawnPoint.rotation;
            }
        }

        SetCrosshairColor(aimingAtEnemy ? enemyCrosshairColor : normalCrosshairColor);
    }

    private void SetCrosshairColor(Color color)
    {
        if (crosshairImage != null)
            crosshairImage.color = color;
    }

    
    private void HandleInput()
    {
        var input = PlayerManager.instance.playerInputs.Attack;

        if (PlayerManager.instance.health == null || PlayerManager.instance.health.currentHealth <= 0)
        {
            if (isSpraying) StopSprayEvent();

            if (weaponAnimator != null)
                weaponAnimator.SetLayerWeight(1, 0f);

            return;
        }

        if (IsPlayerStunned())
        {
            canCombo = false;

            if (weaponAnimator != null)
            {
                weaponAnimator.ResetTrigger(attackTriggerName);
                weaponAnimator.SetLayerWeight(1, 0f);
            }

            return;
        }

        if (weaponAnimator != null)
        {
            weaponAnimator.SetLayerWeight(
                1,
                Mathf.MoveTowards(weaponAnimator.GetLayerWeight(1), 1f, Time.deltaTime * 10f)
            );
        }

        bool cooldownOver = Time.time >= lastAttackTime + attackCooldown;

        if (input.WasPressedThisFrame() && currentAmmo > 0 && cooldownOver)
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger(attackTriggerName);

            lastAttackTime = Time.time;
        }

        if (canCombo && input.IsPressed() && currentAmmo > 0)
        {
            if (weaponAnimator != null)
                weaponAnimator.SetTrigger(attackTriggerName);

            canCombo = false;
        }
    }

    public void StartSprayEvent()
    {
        if (currentAmmo <= 0 || IsPlayerStunned())
            return;

        isSpraying = true;

        if (sprayParticles != null)
            sprayParticles.Play();
    }

    public void StopSprayEvent()
    {
        isSpraying = false;
        canCombo = false;

        if (sprayParticles != null)
            sprayParticles.Stop();

        projectileTimer = 0f;
        ShootProjectileBurst();
    }

    private bool IsPlayerStunned()
    {
        if (PlayerManager.instance == null || PlayerManager.instance.player == null)
            return false;

        PlayerMovement movement = PlayerManager.instance.player.GetComponent<PlayerMovement>();
        if (movement == null)
            return false;

        return movement.isStunned;
    }

    public void OpenComboWindow()
    {
        canCombo = true;
    }

    public void CloseComboWindow()
    {
        canCombo = false;
    }

    private void HandleProjectileSpawning()
    {
        projectileTimer += Time.deltaTime;

        if (projectileTimer >= CurrentProjectileInterval)
        {
            ShootProjectileBurst();
            projectileTimer = 0f;
        }
    }

    private void ShootProjectileBurst()
    {
        if (projectilePrefab == null || nozzleSpawnPoint == null)
            return;

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
        if (rb == null)
            rb = proj.AddComponent<Rigidbody>();

        rb.useGravity = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
        rb.mass = 1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.constraints = RigidbodyConstraints.None;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.linearVelocity = launchDirection.normalized * launchForce;

        PaintGlobs glob = proj.GetComponent<PaintGlobs>();
        if (glob != null)
        {
            glob.InitializeProjectile(rb);
        }

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

        if (currentAmmo <= 0)
            StopSprayEvent();
    }

    public void UpdatePainterColor()
    {
        if (painter != null)
        {
            painter.colorKey = canColorKey;
            painter.UpdateColorFromManager();
        }
    }

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    }

    public float GetAmmoPercentage()
    {
        return maxAmmo <= 0 ? 0f : ((float)currentAmmo / maxAmmo) * 100f;
    }

    public float GetNormalizedAmmo()
    {
        return maxAmmo <= 0 ? 0f : (float)currentAmmo / maxAmmo;
    }
}