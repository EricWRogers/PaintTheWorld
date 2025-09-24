using UnityEngine;
using SuperPupSystems.Helper;
using System.Collections;
using System.Collections.Generic;

public class BushDamage : MonoBehaviour
{
 [Header("Trap Damage (On Touch)")]
    public string targetTag = "Enemy";

    [Header("Weapon Attack Settings")]
    public Transform attackPart;       // The handle/child cube
    public float attackAngle = 45f;    // Swing angle (unused for hardcoded animation)
    public float attackSpeed = 6f;     // Swing speed (unused for hardcoded animation)
    public int weaponDamage = 15;      // Damage dealt per hit
    public float rayLength = 2f;       // Attack reach
    public LayerMask hitMask;          // Who can be hit
    public float comboResetTime = 1f;  // Time allowed between combo clicks (unused)
    public float brushgunDuration = 1.5f; // How long the brushgun attack stays active on click

    [Header("VFX")]
    [Tooltip("The particle system to play during an attack.")]
    public ParticleSystem attackVFX;
    [Tooltip("Radius for the particle system shape (editable at runtime).")]
    public float attackVFXRadius = 2.0f;

    [Header("Animator (for attacks)")]
    public Animator animator;                 
    public string holdAttackBool = "HoldAttack"; 
    public string brushgunBool = "BrushGun";

    private bool isAttacking = false;
    private Quaternion originalRotation;
    private Vector3 originalPosition;

    // Prevent multiple damages in a single hit/frame; cleared by animation at start of each hit window
    private List<GameObject> hitTargets = new List<GameObject>();

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
    }

    void OnTriggerEnter(Collider other)
    {
        if (!isAttacking) return; // Only damage during attack

        if (other.CompareTag(targetTag))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(weaponDamage);
                Debug.Log($"{other.name} took {weaponDamage} damage from collider attack!");
            }
        }
    }

    void Update()
    {
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
        hitTargets.Clear();

        if (attackVFX != null)
        {
            attackVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            attackVFX.Play();
        }

        if (animator != null && !string.IsNullOrEmpty(holdAttackBool))
            animator.SetBool(holdAttackBool, true);

        // Hold while button is down. Actual hits should be invoked from the animation via AnimationHitEvent().
        while (Input.GetButton("Fire1"))
        {
            yield return null;
        }

        // Released: stop the animator bool so it blends back to idle
        if (animator != null && !string.IsNullOrEmpty(holdAttackBool))
            animator.SetBool(holdAttackBool, false);

        // Stop VFX immediately (the animation can also stop it via event if preferred)
        if (attackVFX != null)
            attackVFX.Stop();

        isAttacking = false;
        yield break;
    }

    // Called from Animation Event at the exact hit frame(s)
    public void AnimationHitEvent()
    {
        DoRaycast();
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

    public void ParticleStop()
    {
        if (attackVFX != null)
            attackVFX.Stop();
    }

    // ---------------------------
    // Raycast damage (used by AnimationHitEvent or code)
    // ---------------------------
    void DoRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(attackPart.position, attackPart.forward, out hit, rayLength, hitMask))
        {
            GameObject go = hit.collider.gameObject;
            if (!hitTargets.Contains(go))
            {
                hitTargets.Add(go);
                Health health = go.GetComponent<Health>();
                if (health != null)
                {
                    health.Damage(weaponDamage);
                    Debug.Log($"{go.name} took {weaponDamage} damage from raycast attack!");
                }
            }
        }

        Debug.DrawRay(attackPart.position, attackPart.forward * rayLength, Color.red, 0.05f);
    }
}
