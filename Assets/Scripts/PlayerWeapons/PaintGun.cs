using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class PaintGun : MonoBehaviour
{
    [Header("Weapon Settings")]
    public int weaponDamage = 10;
    [Tooltip("How many shots can be fired per second.")]
    public float fireRate = 5f;
    
    [Header("Ammo & Reload Settings")]
    public int maxAmmo = 30;
    public float reloadTime = 2.0f;
    private int currentAmmo;
    private bool isReloading = false;

    [Header("References")]
    [Tooltip("The bullet prefab to be fired. Must have the 'Bullet' script on it.")]
    public GameObject bulletPrefab;
    [Tooltip("The point from which bullets are spawned.")]
    public Transform bulletSpawnPoint;
    [Tooltip("The animator for the weapon/character.")]
    public Animator animator;

    [Header("Animator Boolean Names")]
    public string isShootingBool = "IsShooting";
    public string isReloadingBool = "IsReloading";

    [Header("VFX & SFX (Optional)")]
    public ParticleSystem muzzleFlash;
    public AudioClip shootingSound;
    public AudioClip reloadSound;
    private AudioSource audioSource;
    
    // Private variables
    private float nextFireTime = 0f;

    void Start()
    {
        // Initialize ammo
        currentAmmo = maxAmmo;

        // Get the AudioSource component on this GameObject
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            // Add an AudioSource if one doesn't exist to prevent errors
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // Ensure animator is assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Prevent any actions if currently reloading
        if (isReloading)
            return;

        // Check for reload input
        // Only reload if the key is pressed AND the magazine isn't already full
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < maxAmmo)
        {
            StartCoroutine(Reload());
            return; // Stop further checks this frame
        }
        
        // Out of ammo check
        if (currentAmmo <= 0)
        {
            // Optionally, you could trigger an "out of ammo" sound or click here
            // And then automatically start reloading if you want
            StartCoroutine(Reload());
            return;
        }

        // Check for shooting input (hold left mouse button for automatic fire)
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            // Set the next time the player can fire
            nextFireTime = Time.time + 1f / fireRate;
            Shoot();
        }
    }

    void Shoot()
    {
        // Trigger shooting animation
        if (animator != null && !string.IsNullOrEmpty(isShootingBool))
            StartCoroutine(ShootingAnimationRoutine());

        // Play VFX and SFX
        if (muzzleFlash != null)
            muzzleFlash.Play();
        
        if (shootingSound != null)
            audioSource.PlayOneShot(shootingSound);
        
        // Decrease ammo count
        currentAmmo--;

        // Instantiate the bullet
        if (bulletPrefab != null && bulletSpawnPoint != null)
        {
            GameObject bulletGO = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            Bullet bullet = bulletGO.GetComponent<Bullet>();
            
            // Set the damage on the spawned bullet from this gun's settings
            if (bullet != null)
            {
                bullet.damage = weaponDamage;
            }
        }
    }

    IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");

        // Trigger the reloading animation
        if (animator != null && !string.IsNullOrEmpty(isReloadingBool))
            animator.SetBool(isReloadingBool, true);
        
        if (reloadSound != null)
            audioSource.PlayOneShot(reloadSound);

        // Wait for the reload duration
        yield return new WaitForSeconds(reloadTime);

        // Stop the reloading animation
        if (animator != null && !string.IsNullOrEmpty(isReloadingBool))
            animator.SetBool(isReloadingBool, false);

        // Refill ammo
        currentAmmo = maxAmmo;
        isReloading = false;
    }

    // A small coroutine to briefly activate the shooting boolean
    IEnumerator ShootingAnimationRoutine()
    {
        animator.SetBool(isShootingBool, true);
        // Wait a very short time before setting it back to false.
        // This makes it act like a trigger, which is good for single-shot animations.
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(isShootingBool, false);
    }
}
