using System.Collections.Generic;
using UnityEngine;
using SuperPupSystems.Helper;

public class FlyingEnemy : Enemy
{
    public CloudNav cloudNav;

    public float minStopDistance;
    public float minPitch = -30f;
    public float maxPitch = 30f;

    private float m_minPitch = -30;
    private float m_maxPitch = 30;

    private float m_curFireTime;
    public LayerMask losMask;

    [Header("Separation")]
    public float seperationDistance;
    public LayerMask enemyMask;
    public int neighborDetectionRange;
    private Collider[] m_neighbors = new Collider[20];

    [Header("Bezier Movement")]
    public float curveSpeed = 2f;
    private float m_curveDistance = 0f;

    private Vector3 m_curveStart;
    private Vector3 m_curveEnd;
    private Vector3 m_curveControl;

    [Header("Tilt")]
    public float bankAmount = 30f;
    public float bankSpeed = 5f;
    private Vector3 lastMoveDir;

    [Header("Pathing")]
    public List<Vector3> path;
    public int startId;
    public int endId;
    public int targetIndex;

    public float speed = 10.0f;
    public bool stopped = false;

    [Header("Stun")]
    public bool stunned;
    public float stunTime;
    private float m_stunTimer;

    private bool recoveringFromStun;
    private float m_recoveryGraceTimer;

    public ParticleSystem stunParticles, stunParticles2;

    private int m_patrolIndex;

    new void Start()
    {
        base.Start();

        m_minPitch = minPitch;
        m_maxPitch = maxPitch;

        lastMoveDir = transform.forward;

        cloudNav = EnemyManager.instance.cloudNav;

        RequestNewPath();

        stunParticles.Stop();
        stunParticles2.Stop();
    }

    void FixedUpdate()
    {
        float playerDist = Vector3.Distance(transform.position, PlayerManager.instance.player.transform.position);

        Vector3 direction = (target.position - transform.position).normalized;

        bool shouldTargetPlayer = Physics.Raycast(transform.position, direction, targetDistance, losMask);

        bool shouldTargetObjective = false;
        if (patroling != null && patroling.paintingObj != null)
        {
            shouldTargetObjective = patroling.paintingObj.percentageCovered > 0f;
        }

        if (shouldTargetPlayer)
        {
            if (target != PlayerManager.instance.player.transform)
            {
                target = PlayerManager.instance.player.transform;
                RequestNewPath();
            }

            targetingPlayer = true;
        }
        else if (shouldTargetObjective)
        {
            if (patroling != null && patroling.paintingObj != null &&
                target != patroling.paintingObj.transform)
            {
                target = patroling.paintingObj.transform;
                RequestNewPath();
            }

            targetingPlayer = false;
        }
        else
        {
            targetingPlayer = false;

            if (patroling != null && patroling.patrolPoints.Count > 0)
            {
                Transform patrolTarget = patroling.patrolPoints[m_patrolIndex];

                if (target != patrolTarget)
                {
                    target = patrolTarget;
                    RequestNewPath();
                }

                if (Vector3.Distance(transform.position, patrolTarget.position) < patrolingDistanceCheck)
                {
                    m_patrolIndex = (m_patrolIndex + 1) % patroling.patrolPoints.Count;
                }
            }
        }
    }

    void RequestNewPath()
    {
        if (cloudNav == null || target == null) return;

        startId = cloudNav.aStar.GetClosestPoint(transform.position);
        endId = cloudNav.aStar.GetClosestPoint(target.position);
        cloudNav.aStar.RequestPath(GetNewPath, startId, endId);
    }

    void GetNewPath(List<Vector3> _path)
    {
        targetIndex = 0;
        path = _path;

        if (path != null && path.Count > 1)
            SetupCurve(path[0], path[1]);
    }

    Vector3 Separation(Vector3 _agentPos, Collider[] _neighbors)
    {
        Vector3 separation = Vector3.zero;
        int count = 0;

        foreach (Collider neighbor in _neighbors)
        {
            if (neighbor == null || neighbor.gameObject == gameObject)
                continue;

            Vector3 toNeighbor = _agentPos - neighbor.transform.position;
            float distance = toNeighbor.magnitude;

            if (distance < seperationDistance && distance > 0f)
            {
                float strength = (seperationDistance - distance) / seperationDistance;
                separation += toNeighbor.normalized * strength;
                count++;
            }
        }

        if (count > 0)
            separation /= count;

        return separation;
    }

    Vector3 GetBezierPoint(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        float u = 1 - t;
        return u * u * a + 2 * u * t * b + t * t * c;
    }

    void SetupCurve(Vector3 start, Vector3 end)
    {
        m_curveStart = start;
        m_curveEnd = end;

        Vector3 dir = (end - start).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up);

        m_curveControl = (start + end) / 2 + perp;
        m_curveDistance = 0f;
    }

    Quaternion GetClampedLookRotation(Vector3 targetPos, float minPitch, float maxPitch)
    {
        Vector3 dir = (targetPos - transform.position).normalized;

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude < 0.001f)
            flatDir = transform.forward;

        Quaternion yawRot = Quaternion.LookRotation(flatDir);

        float pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;
        float clampedPitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        Quaternion pitchRot = Quaternion.Euler(-clampedPitch, 0f, 0f);

        return yawRot * pitchRot;
    }

    public override void Attack()
    {
        anim.SetBool("Attacking", true);
    }

    public void Fire()
    {
        firePoint.LookAt(target);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Bullet>().damage = baseDamage;
        m_curFireTime = attackSpeed;
    }

    public void Stun()
    {
        anim.SetBool("Moving", false);
        anim.SetTrigger("Stun");

        stopped = true;
        recoveringFromStun = false;

        stunParticles.Play();
        stunParticles2.Play();

        m_stunTimer = stunTime + EnemyStunModifier.extraStunTime;
        stunned = true;
    }

    public void Fall()
    {
        GetComponent<Rigidbody>().useGravity = true;
    }
}