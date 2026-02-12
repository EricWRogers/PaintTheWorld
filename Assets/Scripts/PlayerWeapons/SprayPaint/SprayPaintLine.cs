using UnityEngine;

public class SprayPaintLine : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int currentAmmo = 100;
    public int maxAmmo = 200;
    public float consumptionRate = 10f;

    [Header("Effects & References")]
    public ParticleSystem sprayParticles;
    public Transform nozzleSpawnPoint;
    public Transform playerCamera;

    [Header("Aiming Settings")]
    public float maxAimDistance = 20f;
    public float rotationSmoothing = 20f;
    public LayerMask playerMask;

    private float ammoRemainder = 0f;
    private bool isSpraying = false;

    private void Start()
    {
        if (playerCamera == null) playerCamera = Camera.main.transform;

        if (sprayParticles != null)
        {
            var main = sprayParticles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            if (sprayParticles.isPlaying) sprayParticles.Stop();
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void LateUpdate()
    {
        UpdateAimingAndPosition();
    }

    private void UpdateAimingAndPosition()
    {
        if (playerCamera == null || nozzleSpawnPoint == null || sprayParticles == null) return;

        // 1. Position the particles at the nozzle
        sprayParticles.transform.position = nozzleSpawnPoint.position;

        // 2. Create a target point in the world based on camera center
        Ray ray = new Ray(playerCamera.position, playerCamera.forward);
        Vector3 targetPoint;

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, ~playerMask, QueryTriggerInteraction.Ignore))
        {
            targetPoint = hit.point;
        }
        else
        {
            // If hitting nothing, aim at a point 10 units away along the camera's gaze
            targetPoint = ray.GetPoint(10f); 
        }

        // 3. Rotate the spray can towards the targetPoint
        Vector3 direction = (targetPoint - transform.position).normalized;
        
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            
            // Apply rotation to the can (this script's object)
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSmoothing);
            
            // Match particles to the can't orientation
            sprayParticles.transform.rotation = transform.rotation;
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButton(0) && currentAmmo > 0)
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
        }
    }

    private void StopSpraying()
    {
        if (isSpraying)
        {
            isSpraying = false;
            if (sprayParticles != null && sprayParticles.isPlaying) sprayParticles.Stop();
        }
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

    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
    }

    public float GetAmmoPercentage()
    {
        if (maxAmmo <= 0) return 0f;
        return ((float)currentAmmo / maxAmmo) * 100f;
    }
    public float GetNormalizedAmmo()
    {
        if (maxAmmo <= 0) return 0f;
        return (float)currentAmmo / (float)maxAmmo;
    }
}