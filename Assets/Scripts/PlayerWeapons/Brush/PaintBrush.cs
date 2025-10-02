using UnityEngine.Animations;
using SuperPupSystems.Helper;
using UnityEngine;

public class PaintBrush : Weapon
{
    public Transform leftFirePos;
    public Transform rightFirePos;
    public GameObject bullet;
    public GameObject swipeZone;
    [Header("Anim")]
    private Animator anim;
    public string holdAttackBool = "HoldAttack";
    public bool hitEnemy = false;
    public float maxSpeedMult;
    void Awake()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        if (Input.GetButtonDown("Fire1"))
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
    }
    public void ResetAttack()
    {
        hitEnemy = false;
    }

    public void LaunchPaintAtFirepoint(Transform _firePoint)
    {
        PaintGlobs temp = Instantiate(bullet, _firePoint.position, _firePoint.rotation).GetComponent<PaintGlobs>();
        temp.paintColor = player.GetComponent<PlayerPaint>().selectedPaint;
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

        // Get the center and half-extents of the box
        Vector3 boxCenter = swipeZone.transform.position;   // Fixed world position
        Vector3 halfExtents = swipeZone.GetComponent<BoxCollider>().size / 2;                  // half-size of the box
        Quaternion boxRotation = swipeZone.transform.rotation;

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
