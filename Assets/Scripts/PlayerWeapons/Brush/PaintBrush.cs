using UnityEngine.Animations;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEditor.UI;
using UnityEngine.UIElements;

public class PaintBrush : Weapon
{
    public Transform leftFirePos;
    public Transform rightFirePos;
    private Transform m_parentTransform;
    public GameObject bullet;
    public GameObject swipeZone;
    [Header("Anim")]
    public Animator anim;
    public string holdAttackBool = "HoldAttack";
    public bool hitEnemy = false;
    public float maxSpeedMult;

    void Awake()
    {
        m_parentTransform = transform.parent;
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (Input.GetButton("Fire1"))
        {
            Fire();
        }

        if (stateInfo.IsName("BrushAttacks") && !hitEnemy)
            {
                DoBoxCastDamage();
            }

        if (Input.GetButtonUp("Fire1"))
        {
            anim.SetBool(holdAttackBool, false);
        }
    
    }

    public override void Fire()
    {
        anim.SetBool(holdAttackBool, true);
        anim.speed = Mathf.Clamp(attackSpeedMult, 0.5f, maxSpeedMult);
        if (m_parentTransform != null)
        {
            m_parentTransform.rotation = Camera.main.transform.rotation;
        }
    }
    public void ResetAttack()
    {
        hitEnemy = false;
        m_parentTransform.rotation = Quaternion.Euler(0, m_parentTransform.rotation.eulerAngles.y, 0);
    }

    public void LaunchPaintAtFirepoint(Transform _firePoint)
    {
        PaintGlobs temp = Instantiate(bullet, _firePoint.position, _firePoint.rotation).GetComponent<PaintGlobs>();
    }

    public void FireFront()
    {
        LaunchPaintAtFirepoint(firePoint);
    }
    
    public void FireLeft()
    {
        LaunchPaintAtFirepoint(leftFirePos);
    }

    public void FireRight()
    {
        LaunchPaintAtFirepoint(rightFirePos);
    }
    void DoBoxCastDamage()
    {
        if (swipeZone == null) return;
        float verticalAngle = Camera.main.transform.forward.y;

        // Get the center and half-extents of the box
        Vector3 boxCenter = swipeZone.transform.position;   // Fixed world position
        Vector3 halfExtents = swipeZone.GetComponent<BoxCollider>().size / 2;                  // half-size of the box
        Quaternion boxRotation = swipeZone.transform.rotation;

        if (verticalAngle > 0.03101708f)
        {
            boxCenter.y = 1.33f;
        }
        else
        {
            boxCenter.y = -0.19f;
        }


        // Get all colliders in the box
        Collider[] hits = Physics.OverlapBox(
            boxCenter,
            halfExtents,
            boxRotation,
            layerMask
        );

        foreach (Collider col in hits)
        {
            GameObject temp = col.gameObject;
            Health health = temp.GetComponent<Health>();
            if (health != null)
            {
                health.Damage(Mathf.RoundToInt(damage * damageMult));
            }
        }
        hitEnemy = true;
    }
}
