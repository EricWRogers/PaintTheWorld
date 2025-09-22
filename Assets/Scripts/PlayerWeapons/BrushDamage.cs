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

    [Header("Special Attack Swing")]
    public float specialSwingAngle = 45.0f;
    public float specialLiftAngle = 20.0f;
    public float specialSwingSpeed = 4.0f; // For right-click special swing

    [Header("VFX")]
    [Tooltip("The particle system to play during an attack.")]
    public ParticleSystem attackVFX;

    [Header("Animator (for attacks)")]
    public Animator animator;                 // assign in inspector
    public string holdAttackBool = "HoldAttack"; // boolean parameter in Animator that plays the attack animation while true
    public string specialTrigger = "Special"; // optional trigger for right-click special

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

        if (attackVFX != null)
        {
            attackVFX.playOnAwake = false; // ensure it doesn't play at startup
            attackVFX.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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
            // prefer animator trigger if your special animation is in the Animator
            if (animator != null && !string.IsNullOrEmpty(specialTrigger))
            {
                isAttacking = true;
                hitTargets.Clear();
                if (attackVFX != null) attackVFX.Play();
                animator.SetTrigger(specialTrigger);
                // End of special should be signaled by an animation event to EndAttackEvent()
            }
            else
            {
                // fallback to the coroutine if you still want code-driven special
                StartCoroutine(AnimateSpecialSwing());
            }
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

        // End attack state. If your animation has a recovery that must finish before allowing other attacks,
        // use an Animation Event to call EndAttackEvent() instead of immediately clearing isAttacking.
        isAttacking = false;
        yield break;
    }

    // Called from Animation Event at the exact hit frame(s)
    public void AnimationHitEvent()
    {
        // Optionally clear hitTargets at start of each animation hit window via a separate event:
        // Call ClearHitTargets() in animation at the start of the swing if you want to allow fresh hits.
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
    IEnumerator AnimateSpecialSwing()
    {
        isAttacking = true;
        hitTargets.Clear(); // Clear list of enemies already hit

        if (attackVFX != null)
        {
            attackVFX.Play();
        }

        // Define the target rotation for the special swing
        Quaternion targetRotation = originalRotation * Quaternion.Euler(-specialLiftAngle, -specialSwingAngle, 0);

        // --- Animate the swing outwards ---
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * specialSwingSpeed;
            transform.localRotation = Quaternion.Slerp(originalRotation, targetRotation, t);

            DoRaycast(); // Check for hits during the swing
            yield return null;
        }

        // --- Animate the swing back ---
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * specialSwingSpeed;
            transform.localRotation = Quaternion.Slerp(targetRotation, originalRotation, t);
            if (attackVFX != null)
            {
                attackVFX.Stop();
            }
            DoRaycast(); // Check for hits during the return
            yield return null;
        }

        // Ensure the weapon is perfectly reset
        transform.localRotation = originalRotation;
        isAttacking = false;
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
