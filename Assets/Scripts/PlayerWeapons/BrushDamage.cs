using UnityEngine;
using SuperPupSystems.Helper;
using System.Collections;
using System.Collections.Generic;

public class BushDamage : MonoBehaviour
{
    [Header("Weapon Attack Settings")]
    public Transform attackPart;
    public int weaponDamage = 15;
    public float brushgunDuration = 1.5f;
    public string targetTag = "Enemy";
    public LayerMask layerMask;
    public GameObject damageZone;
    public GameObject paintGlobPrefab; // Your PaintGlobs projectile
    public Transform bulletSpawnPoint1; // Where the projectile will be created
    public Transform bulletSpawnPoint2; // Where the projectile will be created 
    public Transform bulletSpawnPoint3; // Where the projectile will be created

    [Tooltip("How often damage is applied while holding the attack (in seconds).")]
    public float damageTickRate = 0.5f; // New variable for damage tick rate

    [Header("VFX")]
    public ParticleSystem attackVFX;
    public float attackVFXRadius = 2.0f;

    [Header("Animator (for attacks)")]
    public Animator animator;
    public string holdAttackBool = "HoldAttack";
    public string brushgunBool = "BrushGun";

    public bool isAttacking = false;
    private List<GameObject> hitTargets = new List<GameObject>();
    private Quaternion originalRotation;
    private Vector3 originalPosition;
    private GameObject m_player;

    void Start()
    {
        if (attackPart == null)
            attackPart = transform;

        originalRotation = attackPart.localRotation;
        originalPosition = attackPart.localPosition;
        attackVFX.GetComponent<ParticlePainter>().particleDamage = weaponDamage;

        if (attackVFX != null)
        {
            var main = attackVFX.main;
            main.playOnAwake = false; // ensure it doesn't play at startup
            attackVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            // Apply configured radius to the Shape module so the particle spread matches the inspector/runtime value
            var shape = attackVFX.shape;
            shape.radius = attackVFXRadius;
        }

        // Ensure animator default state is Idle (set in Animator window)
        if (animator == null)
        {
            // Try to get an Animator on this GameObject if not assigned
            animator = GetComponent<Animator>();
        }
        m_player = PlayerManager.instance.player;
    }

    void Update()
    {
        weaponDamage = PlayerManager.instance.player.GetComponent<PlayerWeapon>().damage;
        // Hold-to-attack: start when Fire1 pressed, stop when released
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            StartCoroutine(HoldAttackCoroutine());
        }

        // Right-click special (uses existing coroutine or animator trigger)
        if (Input.GetButtonDown("Fire2") && !isAttacking)
        {
            StartCoroutine(BrushGun());
        }
    }

    // Starts the animator boolean and VFX; animation should call AnimationHitEvent() via Animation Events
    IEnumerator HoldAttackCoroutine()
    {
        isAttacking = true;
    hitTargets.Clear(); // Clear at the very start

    if (attackVFX != null)
    {
        attackVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        attackVFX.Play();
    }

    if (animator != null && !string.IsNullOrEmpty(holdAttackBool))
        animator.SetBool(holdAttackBool, true);
    
    // --- MODIFIED SECTION ---
    float nextDamageTime = 0f; // Timer for controlling damage rate

    // Hold while button is down
    while (Input.GetButton("Fire1"))
    {
            // Check if enough time has passed to deal damage again
            if (Time.time >= nextDamageTime)
            {
                // IMPORTANT: Clear the list before each cast. This allows an enemy
                // to be hit again on the next damage tick if they are still in range.
                hitTargets.Clear();

                // Perform the box cast to deal damage
                DoBoxCastDamage();

                // Set the time for the next possible damage tick
                nextDamageTime = Time.time + damageTickRate;

                Glob(); // Call the Glob method to shoot paint globs
             }

        yield return null; // Wait for the next frame
    }
    // --- END OF MODIFIED SECTION ---

    // Released: stop the animator bool so it blends back to idle
    if (animator != null && !string.IsNullOrEmpty(holdAttackBool))
        animator.SetBool(holdAttackBool, false);

    // Stop VFX immediately
    if (attackVFX != null)
        attackVFX.Stop();

    isAttacking = false;
    }
    // Optional helper: call from animation at moment you want to allow the same targets to be hit again
    public void ClearHitTargets()
    {
        hitTargets.Clear();
    }

    // Optional helper: call from animation at the end of the special/attack clip to re-enable input
    public void EndAttackEvent()
    {
        isAttacking = false;
        if (attackVFX != null)
            attackVFX.Stop();
    }

    // ---------------------------
    // Existing code-driven special (kept as fallback)
    // ---------------------------
    IEnumerator BrushGun()
    {
        isAttacking = true;
        hitTargets.Clear();

        if (attackVFX != null)
        {
            attackVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            attackVFX.Play();
        }

        if (animator != null && !string.IsNullOrEmpty(brushgunBool))
            animator.SetBool(brushgunBool, true);

        // Click-based attack: play animation/VFX for a short configurable duration.
        // Animation should call AnimationHitEvent() at the correct frame(s). After duration, return to idle.
        yield return new WaitForSeconds(brushgunDuration);

        // End the brushgun state
        if (animator != null && !string.IsNullOrEmpty(brushgunBool))
            animator.SetBool(brushgunBool, false);

        if (attackVFX != null)
            attackVFX.Stop();

        isAttacking = false;
        yield break;
    }

    public void ParticleStart()
    {
        if (attackVFX != null)
        {
            // Ensure radius is applied in case it was changed at runtime
            var shape = attackVFX.shape;
            shape.radius = attackVFXRadius;

            attackVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            attackVFX.Play();
        }
    }

    public void Glob()
    {
        if (paintGlobPrefab != null && bulletSpawnPoint1 != null && bulletSpawnPoint2 != null && bulletSpawnPoint3 != null)
        {
            GameObject glob1 = Instantiate(paintGlobPrefab, bulletSpawnPoint1.position, bulletSpawnPoint1.rotation);
            GameObject glob2 = Instantiate(paintGlobPrefab, bulletSpawnPoint2.position, bulletSpawnPoint2.rotation);
            GameObject glob3 = Instantiate(paintGlobPrefab, bulletSpawnPoint3.position, bulletSpawnPoint3.rotation);
            PaintGlobs temp1 = glob1.GetComponent<PaintGlobs>();
            PaintGlobs temp2 = glob2.GetComponent<PaintGlobs>();
            PaintGlobs temp3 = glob3.GetComponent<PaintGlobs>();
            temp1.paintColor = m_player.GetComponent<PlayerPaint>().selectedPaint;
            temp1.bulletDamage = m_player.GetComponent<PlayerWeapon>().damage;
            temp1.radius = m_player.GetComponent<PlayerWeapon>().paintRadius;

            temp2.paintColor = m_player.GetComponent<PlayerPaint>().selectedPaint;
            temp2.bulletDamage = m_player.GetComponent<PlayerWeapon>().damage;
            temp2.radius = m_player.GetComponent<PlayerWeapon>().paintRadius;

            temp3.paintColor = m_player.GetComponent<PlayerPaint>().selectedPaint;
            temp3.bulletDamage = m_player.GetComponent<PlayerWeapon>().damage;
            temp3.radius = m_player.GetComponent<PlayerWeapon>().paintRadius;
        }
    }

    public void ParticleStop()
    {
        if (attackVFX != null)
            attackVFX.Stop();
    }

    // ---------------------------
    // Box-cast damage (used by AnimationHitEvent or code)
    // ---------------------------


    void DoBoxCastDamage()
    {
        if (damageZone == null) return;

        // Get the center and half-extents of the box
        Vector3 boxCenter = damageZone.transform.position;   // Fixed world position
        Vector3 halfExtents = damageZone.GetComponent<BoxCollider>().size / 2;                  // half-size of the box
        Quaternion boxRotation = damageZone.transform.rotation;

        // Get all colliders in the box
        Collider[] hits = Physics.OverlapBox(
            boxCenter,
            halfExtents,
            boxRotation,
            layerMask
        );

        foreach (Collider col in hits)
        {
            if (col.CompareTag(targetTag))
            {
                GameObject go = col.gameObject;
                if (!hitTargets.Contains(go))
                {
                    hitTargets.Add(go);

                    Health health = go.GetComponent<Health>();
                    if (health != null)
                    {
                        health.Damage(weaponDamage);
                        Debug.Log($"{go.name} took {weaponDamage} damage!");
                    }
                }
            }
        }
    }
    // if (damageZone.GetComponent<AttackZone>().enemyInZone == true)
    // {
    //     foreach (GameObject go in damageZone.GetComponent<AttackZone>().enemiesInZone)
    //     {
    //         if (!hitTargets.Contains(go))
    //         {
    //             hitTargets.Add(go);
    //             Health health = go.GetComponent<Health>();
    //             if (health != null)
    //             {
    //                 health.Damage(weaponDamage);
    //                 Debug.Log($"{go.name} took {weaponDamage} damage from BoxCast attack!");
    //             }
    //         }

    //     }
    // }
    //}
    
}
