using UnityEngine;
using SuperPupSystems.Helper;
using System.Collections;

public class BushDamage : MonoBehaviour
{
    [Header("Trap Damage (On Touch)")]
    public int trapDamage = 10;
    public string targetTag = "Player";

    [Header("Handle Weapon Attack")]
    public Transform attackPart;       
    public float attackAngle = 45f;
    public float attackSpeed = 5f;
    public int weaponDamage = 15;

    private bool isAttacking = false;
    private Quaternion originalRotation;

    void Start()
    {
        if (attackPart == null)
            attackPart = transform;

        originalRotation = attackPart.localRotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(trapDamage);
                Debug.Log($"{other.name} took {trapDamage} damage from bush trap!");
            }
        }
    }

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && !isAttacking) // Left Mouse Button
        {
            StartCoroutine(DiagonalAttack());
        }
    }

    IEnumerator DiagonalAttack()
    {
        isAttacking = true;

        Quaternion targetRot = originalRotation * Quaternion.Euler(attackAngle, attackAngle, 0);

        // Swing forward
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * attackSpeed;
            attackPart.localRotation = Quaternion.Slerp(originalRotation, targetRot, t);
            yield return null;
        }

        // Swing back
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * attackSpeed;
            attackPart.localRotation = Quaternion.Slerp(targetRot, originalRotation, t);
            yield return null;
        }

        attackPart.localRotation = originalRotation;
        isAttacking = false;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (!isAttacking) return; // only deal damage while swinging

        if (collision.collider.CompareTag(targetTag))
        {
            Health health = collision.collider.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(weaponDamage);
                Debug.Log($"{collision.collider.name} took {weaponDamage} damage from handle swing!");
            }
        }
    }
}
