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
    public float attackAngle = 45f;    // Swing angle
    public float attackSpeed = 6f;     // Swing speed
    public int weaponDamage = 15;      // Damage dealt per hit
    public float rayLength = 2f;       // Attack reach
    public LayerMask hitMask;          // Who can be hit
    public float comboResetTime = 2f;  // Time allowed between combo clicks
    
    [Header("Special Attack Swing")]
    public float specialSwingAngle = 45.0f;
    public float specialLiftAngle = 20.0f;
    public float specialSwingSpeed = 4.0f; // <-- For your right-click attack

    [Header("VFX")]
    [Tooltip("The particle system to play during an attack.")]
    public ParticleSystem attackVFX;

    private bool isAttacking = false;
    private int comboStep = 0;         // 0 = first attack
    private float lastClickTime = 0f;
    private Quaternion originalRotation;
    private Vector3 originalPosition;

     private List<GameObject> hitTargets = new List<GameObject>();

    void Start()
    {
        if (attackPart == null)
            attackPart = transform;

        originalRotation = attackPart.localRotation;
        originalPosition = attackPart.localPosition;
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
    // ---------------------------
    // Handle LMB combo system
    // ---------------------------
    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            float timeSinceLastClick = Time.time - lastClickTime;

            if (timeSinceLastClick > comboResetTime)
            {
                comboStep = 0; // reset combo if too slow
            }

            if (comboStep == 0)
                StartCoroutine(SlashAttack(-attackAngle, -attackAngle));   // Left Diagonal
            else if (comboStep == 1)
                StartCoroutine(SlashAttack(-attackAngle, attackAngle));   // Right Diagonal
            else if (comboStep == 2)
                StartCoroutine(PokeAttack());                             // Poke

            comboStep = (comboStep + 1) % 3; // cycle back to first after third attack
            lastClickTime = Time.time;
        }

    if (Input.GetButtonDown("Fire2") && !isAttacking)
    {
        // Call the new diagonal swing coroutine
        StartCoroutine(AnimateSpecialSwing()); 
        comboStep = 0; // Reset combo after a special attack
    }
    }

    // ---------------------------
    // Slash attack (diagonal)
    // ---------------------------
    IEnumerator SlashAttack(float xAngle, float yAngle)
    {
        isAttacking = true;

        if (attackVFX != null)
        {
            attackVFX.Play();
        }
      

        Quaternion targetRot = originalRotation * Quaternion.Euler(xAngle, yAngle, 0);
        Vector3 originalPos = attackPart.localPosition;
        Vector3 backwardsPos = originalPos - attackPart.forward * 0.5f; // Move 0.5 units forward

        DoRaycast();

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * attackSpeed;
            attackPart.localRotation = Quaternion.Slerp(originalRotation, targetRot, t);
            attackPart.localPosition = Vector3.Lerp(originalPos, backwardsPos, t);
            DoRaycast();
           
            yield return null;
        }

        // Return
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * attackSpeed;
            attackPart.localRotation = Quaternion.Slerp(targetRot, originalRotation, t);
            attackPart.localPosition = Vector3.Lerp(backwardsPos, originalPos, t);

            if (attackVFX != null)
            {
                attackVFX.Stop();
            }

            DoRaycast();
            yield return null;
        }

        attackPart.localRotation = originalRotation;
        attackPart.localPosition = originalPos;
        isAttacking = false;
    }

    IEnumerator PokeAttack()
    {
        isAttacking = true;

        if (attackVFX != null)
        {
            attackVFX.Play();
        }

        Vector3 originalPos = attackPart.localPosition;
        Vector3 backwardsPos = originalPos - attackPart.forward * 0.5f;

        // Check for hit at initial position

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * attackSpeed;
            attackPart.localPosition = Vector3.Lerp(originalPos, backwardsPos, t);
            DoRaycast();
            yield return null;
        }

        // Return
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * attackSpeed;
            attackPart.localPosition = Vector3.Lerp(backwardsPos, originalPos, t);

            if (attackVFX != null)
            {
                attackVFX.Stop();
            }

            DoRaycast();
            yield return null;
        }

        attackPart.localPosition = originalPos;
        isAttacking = false;
    }

    // ---------------------------
    // Raycast damage
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

    void DoRaycast()
    {
        RaycastHit hit;
        if (Physics.Raycast(attackPart.position, attackPart.forward, out hit, rayLength, hitMask))
        {
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(weaponDamage);
                Debug.Log($"{hit.collider.name} took {weaponDamage} damage from raycast attack!");
            }
        }

        Debug.DrawRay(attackPart.position, attackPart.forward * rayLength, Color.red, 0.05f);
    }
}
