using System.Collections;
using UnityEngine;
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

    private float ammoRemainder = 0f;
    private bool isSpraying = false;
    private float projectileTimer = 0f;

    [Header("Paint Settings")]
    public int canColorKey = 0;
    private ParticlePainter painter;

    public int ProjectileCount => Mathf.Max(1, 1 + bonusProjectileCount);

    [Header("Animation Settings")]
    public Animator weaponAnimator;
    private string attackTriggerName = "AttackTrigger";
    private bool canCombo = false;

    public float CurrentProjectileInterval
    {
        get
        {
            float mult = Mathf.Max(0.1f, bonusAttackSpeedMultiplier);
            return projectileInterval / mult;
        }
    }

   // [Header("Crosshair UI")]
   // public RectTransform crosshairUI;

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

        if (isSpraying && currentAmmo > 0)
        {
            ConsumeAmmo();
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

    public void SetAttackSpeedMultiplier(float multiplier)
    {
        bonusAttackSpeedMultiplier = Mathf.Max(1f, multiplier);
    }

    private void UpdateAimingAndPosition()
    {
        if (playerCamera == null || nozzleSpawnPoint == null || sprayParticles == null) return;

        sprayParticles.transform.position = nozzleSpawnPoint.position;

        Vector3 rayOrigin = playerCamera.position + (playerCamera.forward * 0.5f);
        Ray ray = new Ray(rayOrigin, playerCamera.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, ~playerMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;

      //      if(crosshairUI != null && hit.collider.CompareTag("Enemy")) 
    //        {
    //            crosshairUI.GetComponent<UnityEngine.UI.Image>().color = Color.red;
    //        }
        }
        else
        {
            targetPoint = ray.GetPoint(100f);
    //        if(crosshairUI != null) 
    //        {
    //            crosshairUI.GetComponent<UnityEngine.UI.Image>().color = Color.black;
    //        }
        }

        Vector3 direction = (targetPoint - transform.position).normalized;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            nozzleSpawnPoint.rotation = Quaternion.Slerp(nozzleSpawnPoint.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
            sprayParticles.transform.rotation = transform.rotation;
        }

    //    if (crosshairUI != null)
  //      {
  //          Vector3 screenPoint = Camera.main.WorldToScreenPoint(targetPoint);
  //          if (screenPoint.z < 0) {
  //          screenPoint = new Vector3(-1000, -1000, 0);
  //      }
  //          crosshairUI.position = screenPoint;
  //      }
    }

    private void HandleInput()
    {
        var input = PlayerManager.instance.playerInputs.Attack;

        bool isAlreadyAttacking = weaponAnimator.GetCurrentAnimatorStateInfo(1).IsTag("Attack");

        if (input.WasPressedThisFrame() && currentAmmo > 0)
        {
                weaponAnimator.SetTrigger(attackTriggerName);
        }

        if (canCombo && input.IsPressed() && currentAmmo > 0)
        {
            weaponAnimator.SetTrigger(attackTriggerName);

            canCombo = false; 
        }
}

    public void StartSprayEvent()
    {
        if (currentAmmo <= 0) return;
        
        isSpraying = true;
        if (sprayParticles != null) sprayParticles.Play();
        
    }

    public void StopSprayEvent()
    {
        isSpraying = false;
        canCombo = false;
        if (sprayParticles != null) sprayParticles.Stop();
        projectileTimer = 0f;
        ShootProjectileBurst();
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
        
        if (currentAmmo <= 0) StopSprayEvent();
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