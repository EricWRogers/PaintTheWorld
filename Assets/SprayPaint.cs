using UnityEngine;
using System.Collections;

public class SprayPaint : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int currentAmmo = 100;
    public int maxAmmo = 200;
    public float consumptionRate = 10f; // Ammo used per second of spraying

    [Header("Effects")]
    public ParticleSystem sprayParticles;
    
    private float ammoRemainder = 0f;
    private bool isSpraying = false;

    private void Start()
    {
        // Ensure particles are off at the start
        if (sprayParticles != null && sprayParticles.isPlaying)
        {
            sprayParticles.Stop();
        }
    }

    private void Update()
    {
        HandleInput();
    }

    private void HandleInput()
    {
        // Check if Left Mouse is held down and we have ammo
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
            if (sprayParticles != null && !sprayParticles.isPlaying)
            {
                sprayParticles.Play();
            }
        }
    }

    private void StopSpraying()
    {
        if (isSpraying)
        {
            isSpraying = false;
            if (sprayParticles != null && sprayParticles.isPlaying)
            {
                sprayParticles.Stop();
            }
        }
    }

    private void ConsumeAmmo()
    {
        // Using Time.deltaTime ensures ammo consumption is consistent regardless of frame rate
        ammoRemainder += consumptionRate * Time.deltaTime;

        if (ammoRemainder >= 1f)
        {
            int ammoToSubtract = Mathf.FloorToInt(ammoRemainder);
            currentAmmo -= ammoToSubtract;
            ammoRemainder -= ammoToSubtract;

            // Clamp ammo so it doesn't go below zero
            currentAmmo = Mathf.Max(0, currentAmmo);
        }
    }

    // This is the method called by your AmmoStation script
    public void AddAmmo(int amount)
    {
        currentAmmo += amount;
        currentAmmo = Mathf.Clamp(currentAmmo, 0, maxAmmo);
        Debug.Log($"Ammo refilled! Current ammo: {currentAmmo}");
    }
}