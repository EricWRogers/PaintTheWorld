using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class PaintGun : MonoBehaviour
{
     [Header("Weapon Settings")]
    public GameObject paintGlobPrefab; // Your PaintGlobs projectile
    public Transform bulletSpawnPoint; // Where the projectile will be created
    public float fireRate = 0.5f;
    public int magazineSize = 10;
    public float reloadTime = 1.5f;

    [Header("Animator")]
    public Animator animator;
    public string shootingBool = "IsShooting";
    public string reloadingBool = "IsReloading";

    private int currentAmmo;
    private bool isReloading = false;
    private float nextFireTime = 0f;

    void Start()
    {
        currentAmmo = magazineSize;
        // Automatically get the Animator component if not assigned
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        // Don't allow any actions if reloading
        if (isReloading)
        {
            return;
        }

        // Left-click to shoot
        if (Input.GetButton("Fire1") && Time.time >= nextFireTime)
        {
            if (currentAmmo > 0)
            {
                nextFireTime = Time.time + fireRate;
                StartCoroutine(Shoot());
            }
            else // Automatically reload if out of ammo
            {
                StartCoroutine(Reload());
            }
        }

        // Press 'R' to reload
        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
        }
    }

    IEnumerator Shoot()
    {
        // Trigger the shooting animation
        animator.SetBool(shootingBool, true);

        // Create the PaintGlob projectile
        if (paintGlobPrefab != null && bulletSpawnPoint != null)
        {
            Instantiate(paintGlobPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        }

        currentAmmo--;

        // Wait for a moment before setting the animation boolean back to false
        yield return new WaitForSeconds(0.1f);
        animator.SetBool(shootingBool, false);
    }

    IEnumerator Reload()
    {
        isReloading = true;
        // Trigger the reloading animation
        animator.SetBool(reloadingBool, true);

        // Wait for the reload duration
        yield return new WaitForSeconds(reloadTime);

        // Refill ammo and end the reloading state
        currentAmmo = magazineSize;
        animator.SetBool(reloadingBool, false);
        isReloading = false;
    }
}
