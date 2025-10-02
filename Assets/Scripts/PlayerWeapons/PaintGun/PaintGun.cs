using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class PaintGun : MonoBehaviour
{
    [Header("Weapon Settings")]
    public GameObject paintGlobPrefab;
    public Transform bulletSpawnPoint;
    [Tooltip("The time in seconds between each shot in the continuous stream. A smaller number means a faster stream.")]
    public float timeBetweenShots = 0.1f; // Controls the speed of the stream
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
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    void Update()
    {
        if (isReloading)
        {
            return;
        }


        if (Input.GetKeyDown(KeyCode.R) && currentAmmo < magazineSize)
        {
            StartCoroutine(Reload());
            return;
        }

        // --- Continuous Shooting Logic ---
        if (Input.GetButton("Fire1") && currentAmmo > 0)
        {
            // If holding the button and have ammo, set the animation to shooting
          //  animator.SetBool(shootingBool, true);

            // Fire projectiles on a timer to create a stream
            if (Time.time >= nextFireTime)
            {
                nextFireTime = Time.time + timeBetweenShots;
                Shoot();
            }
        }
        else // This block runs if the button is released OR if ammo is zero
        {
            // Stop the shooting animation
         //   animator.SetBool(shootingBool, false);
        }

        // --- Auto-Reload Logic ---
        // If holding the button but out of ammo, start reloading
        if (Input.GetButton("Fire1") && currentAmmo <= 0)
        {
            StartCoroutine(Reload());
        }
    }

    void Shoot()
    {
        if (paintGlobPrefab != null && bulletSpawnPoint != null)
        {
            Instantiate(paintGlobPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
        }
        currentAmmo--;
    }

    IEnumerator Reload()
    {
        isReloading = true;
        
        // IMPORTANT: Ensure the shooting animation stops when reloading begins
      //  animator.SetBool(shootingBool, false);
     //   animator.SetBool(reloadingBool, true);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = magazineSize;
    //    animator.SetBool(reloadingBool, false);
        isReloading = false;
    }
}
