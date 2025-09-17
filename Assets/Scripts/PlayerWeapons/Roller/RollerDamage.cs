using UnityEngine;
using SuperPupSystems.Helper;
using System.Collections;
using System.Collections.Generic;

public class RollerDamage : MonoBehaviour
{
    [Header("Weapon Attack Settings")]
    public Transform attackPart;   // Weapon tip / child object
    public int weaponDamage = 25;  // Damage dealt per hit
    public float rayLength = 2f;   // Attack reach
    public LayerMask hitMask;      // Who can be hit

    private Animator anim;
    private bool isAttacking = false;

    void Start()
    {
        if (attackPart == null)
            attackPart = transform;

        anim = GetComponent<Animator>();
    }

    void Update()
    {
        // Player pressed attack
        if (Input.GetButtonDown("Fire1") && !isAttacking)
        {
            isAttacking = true;

            // Tell animator to start combo
            anim.SetBool("IsAttacking", true);
            anim.SetTrigger("M1");
        }
    }

    // ---------------------------
    // Called from Animation Events
    // ---------------------------

    // Triggered mid-swing (add event in animation)
    public void DealDamage()
    {
        RaycastHit hit;
        if (Physics.Raycast(attackPart.position, attackPart.forward, out hit, rayLength, hitMask))
        {
            Health health = hit.collider.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(weaponDamage);
                Debug.Log($"{hit.collider.name} took {weaponDamage} damage!");
            }
        }

        Debug.DrawRay(attackPart.position, attackPart.forward * rayLength, Color.red, 0.2f);
    }

    // Triggered at end of each attack animation (add event in animation)
    public void EndAttack()
    {
        isAttacking = false;
        anim.SetBool("IsAttacking", false);
    }
}