using System.Collections.Generic;
using UnityEngine;
using SuperPupSystems.Helper;

public class FlyingEnemy : Enemy
{
    public CloudNav cloudNav;

    public float minStopDistance;
    public float minHeightFromPLayerToShoot;
    public float minPitch = -30f;
    public float maxPitch = 30f;

    private float m_minPitch = -30;
    private float m_maxPitch = 30;
    private Quaternion lookRot;
    private float m_curFireTime;

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

    Vector3 ZeroY(Vector3 _vector)
    {
        return new Vector3(_vector.x, 0.0f, _vector.z);
    }

    new void Start()
    {
        base.Start();
        m_minPitch = minPitch;
        m_maxPitch = maxPitch;
        lastMoveDir = transform.forward;
        ChooseTarget();
        cloudNav = EnemyManager.instance.cloudNav;
        RequestNewPath();
        stunParticles.Stop();
        stunParticles2.Stop();
    }

    void FixedUpdate()
    {
        if (cloudNav == null) cloudNav = EnemyManager.instance.cloudNav;
        if (target == null) return;

        if (!target.parent.gameObject.activeInHierarchy)
        {
            target = EnemyManager.instance.GetObjectiveTarget().transform;
        }
        
        //stunning
        if (stunned)
        {
            m_minPitch = -90;
            m_maxPitch = 90;
            m_stunTimer -= Time.deltaTime;

            if (m_stunTimer <= 0f)
            {
                GetComponent<Health>().Revive();
                anim.SetTrigger("Unstun");
                GetComponent<Rigidbody>().useGravity = false;
                stunned = false;
                recoveringFromStun = true;
                m_recoveryGraceTimer = EnemyStunModifier.extraRecoveryGrace;
                stunParticles.Stop();
                stunParticles2.Stop();
                // Trigger the stun-recovery splash item
                GameEvents.EnemyRecoveredFromStun?.Invoke(gameObject);
            }

            return;
        }

        //recovery grace
        if (recoveringFromStun)
        {
            m_recoveryGraceTimer -= Time.deltaTime;


            if (m_recoveryGraceTimer <= 0f)
            {
                recoveringFromStun = false;
                stopped = false;

            }

            return;
        }

        
        if (path == null || path.Count == 0)
        {
            RequestNewPath();
            return;
        }

        float distance = Vector3.Distance(transform.position, target.transform.position);
        
        stopped = distance <= minStopDistance;
        bool inAttackRange = distance <= attackRange;
        if(!inAttackRange || !stunned)
        {
            anim.SetBool("Moving", true);
        }
        if (m_curveDistance == 0f && path.Count > 1 && targetIndex == 1) SetupCurve(path[0], path[1]);
        
        if (m_curveDistance >= 1f)
        {
            targetIndex++;
            if (targetIndex >= path.Count && !inAttackRange)
            {
                RequestNewPath();
                return;
            }
            //SetupCurve(transform.position, path[targetIndex]);
        }


        if(inAttackRange && transform.rotation.z <= 1f && transform.rotation.z >= -1f)
        {
            anim.SetBool("Moving", false);
            Attack();
            return;
        }

        float segmentDistance = Vector3.Distance(m_curveStart, m_curveEnd);

        if (segmentDistance < 0.1f) m_curveDistance = 1f;
        else m_curveDistance += (curveSpeed / segmentDistance) * Time.fixedDeltaTime;

        Vector3 nextPos = GetBezierPoint(m_curveStart, m_curveControl, m_curveEnd, m_curveDistance);

        Vector3 separation = Vector3.zero;
        int count = Physics.OverlapSphereNonAlloc(transform.position, neighborDetectionRange, m_neighbors, enemyMask);
        
        if (count > 0) separation = Separation(transform.position, m_neighbors);

        Vector3 moveTarget = nextPos + separation * 5f;

        Quaternion targetRot;

        if (inAttackRange)
        {
            Vector3 flatDir = target.position;
            flatDir.y = transform.position.y;
            Vector3 dir = (flatDir - transform.position).normalized;
            if (dir.sqrMagnitude < 0.001f)
                dir = transform.forward;
            dir.z = 0f;
            targetRot = Quaternion.LookRotation(dir);
            
        }
        else
        {
            targetRot = GetClampedLookRotation(target.position, m_minPitch, m_maxPitch);
        }

        Vector3 moveDir = (nextPos - transform.position).normalized + separation * 3f;

        lastMoveDir = Vector3.Lerp(lastMoveDir, moveDir, Time.fixedDeltaTime * 5f);

        Quaternion bankRot = Quaternion.AngleAxis(
            Mathf.Clamp(
                -Vector3.SignedAngle(transform.forward, lastMoveDir, Vector3.up),
                -bankAmount,
                bankAmount
            ),
            Vector3.forward
        );

        Quaternion finalRot = targetRot * bankRot;

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            finalRot,
            Time.fixedDeltaTime * bankSpeed
        );

        transform.position = Vector3.Lerp(
            transform.position,
            moveTarget,
            Time.fixedDeltaTime * speed
        );
    }

    void RequestNewPath()
    {
        startId = cloudNav.aStar.GetClosestPoint(transform.position);
        endId = cloudNav.aStar.GetClosestPoint(target.transform.position);
        cloudNav.aStar.RequestPath(GetNewPath, startId, endId);
    }

    void GetNewPath(List<Vector3> _path)
    {
        targetIndex = 0;
        path = _path;
        SetupCurve(path[0], path[1]);
        Debug.Log("PATH RECEIVED: " + (_path != null ? _path.Count.ToString() : "NULL"));
    }

    Vector3 Separation(Vector3 _agentPos, Collider[] _neighbors)
    {
        Vector3 separation = Vector3.zero;
        int count = 0;

        foreach (Collider neighbor in _neighbors)
        {
            if (neighbor == null) continue;
            if (neighbor.gameObject == gameObject) continue;

            Vector3 toNeighbor = _agentPos - neighbor.transform.position;
            float distance = toNeighbor.magnitude;

            if (distance < seperationDistance && distance > 0f)
            {
                float strength = (seperationDistance - distance) / seperationDistance;
                separation += toNeighbor.normalized * strength;
                count++;
            }
        }
        if(count > 0)
            separation /= count;

        return separation;
    }

    Vector3 GetBezierPoint(Vector3 _startPoint, Vector3 _midPoint, Vector3 _endPoint, float _distance)
    {
        float u = 1 - _distance;
        return u * u * _startPoint +
            2 * u * _distance * _midPoint +
            _distance * _distance * _endPoint;
    }
    void SetupCurve(Vector3 _start, Vector3 _end)
    {
        m_curveStart = _start;
        m_curveEnd = _end;

        Vector3 dir = (_end - _start).normalized;
        Vector3 perp = Vector3.Cross(dir, Vector3.up);

        m_curveControl = (_start + _end) / 2 + perp;

        m_curveDistance = 0f;
    }
    Quaternion GetClampedLookRotation(Vector3 _targetPos, float _minPitch, float _maxPitch)
    {
        Vector3 dir = (_targetPos - transform.position).normalized;

        Vector3 flatDir = new Vector3(dir.x, 0f, dir.z);
        if (flatDir.sqrMagnitude < 0.001f)
            flatDir = transform.forward;

        Quaternion yawRot = Quaternion.LookRotation(flatDir);

        float pitch = Mathf.Asin(dir.y) * Mathf.Rad2Deg;

        float clampedPitch = Mathf.Clamp(pitch, _minPitch, _maxPitch);

        Quaternion pitchRot = Quaternion.Euler(-clampedPitch, 0f, 0f);

        return yawRot * pitchRot;
    }

    public void ChooseTarget()
    {
        if(EnemyManager.instance.maxFlyingTargetingPlayer > EnemyManager.instance.flyingTargetingPlayer)
        {
            targetingPlayer = true;
            target = PlayerManager.instance.player.transform;
            EnemyManager.instance.flyingTargetingPlayer++;
        }
        else
        {
            targetingPlayer = false;
            target = EnemyManager.instance.GetObjectiveTarget().transform;
        }
    }

    public void Stun()
    {
        anim.SetBool("Moving", false);
        anim.SetTrigger("Stun");
        stopped = true;
        recoveringFromStun = false;
        stunParticles.Play();
        stunParticles2.Play();
        //grace period item
        m_stunTimer = stunTime + EnemyStunModifier.extraStunTime;
        stunned = true;

    }

    public override void Attack()
    {
        anim.SetBool("Attacking", true);
    }

    public void Fire()
    {
        firePoint.LookAt(target.transform);
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Bullet>().damage = baseDamage;
        m_curFireTime = attackSpeed;
    }
    public void Fall()
    {
        GetComponent<Rigidbody>().useGravity = true;
    }
}