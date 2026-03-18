using System.Collections.Generic;
using UnityEngine;
using SuperPupSystems.Helper;

public class FlyingEnemy : Enemy
{
    public CloudNav cloudNav;
    public float minStopDistance;
    public float minHeightFromPLayerToShoot;

    private float m_curFireTime;
    private GameObject m_player;

    public List<Vector3> path;
    public int startId;
    public int endId;
    public int targetIndex;
    public float speed = 100.0f;
    public bool stopped = false;

    [Header("Stun")]
    public bool stunned;
    public float stunTime;
    private float m_stunTimer;

    private bool recoveringFromStun;
    private float m_recoveryGraceTimer;

    Vector3 ZeroY(Vector3 _vector)
    {
        return new Vector3(_vector.x, 0.0f, _vector.z);
    }

    new void Start()
    {
        base.Start();
        m_player = PlayerManager.instance.player;
        RequestNewPath();
    }

    void FixedUpdate()
    {
        //stunning
        if (stunned)
        {
            m_stunTimer -= Time.deltaTime;

            Debug.Log($"[FlyingEnemy] STUNNED | timer={m_stunTimer:F2}");

            if (m_stunTimer <= 0f)
            {
                modelMeshRenderer.materials[1].color = Color.clear;
                GetComponent<Health>().Revive();

                stunned = false;
                recoveringFromStun = true;
                m_recoveryGraceTimer = EnemyStunModifier.extraRecoveryGrace;

                Debug.Log($"[FlyingEnemy] Recovered from stun, entering grace for {m_recoveryGraceTimer:F2}s");

                // Trigger the stun-recovery splash item
                GameEvents.EnemyRecoveredFromStun?.Invoke(gameObject);
            }

            return;
        }

        //recovery grace
        if (recoveringFromStun)
        {
            m_recoveryGraceTimer -= Time.deltaTime;

            Debug.Log($"[FlyingEnemy] RECOVERY GRACE | timer={m_recoveryGraceTimer:F2}");

            if (m_recoveryGraceTimer <= 0f)
            {
                recoveringFromStun = false;
                stopped = false;

                Debug.Log("[FlyingEnemy] Grace period ended, resuming behavior.");
            }

            return;
        }

        
        if (path == null || path.Count == 0)
        {
            RequestNewPath();
            return;
        }

        float distance = Vector3.Distance(transform.position, m_player.transform.position);

        if (distance <= minStopDistance)
        {
            stopped = true;
        }

        if (distance <= attackRange && stopped)
        {
            transform.LookAt(m_player.transform.position);
            Attack();
        }
        else
        {
            stopped = false;

            if (targetIndex >= path.Count)
            {
                RequestNewPath();
                return;
            }

            if (transform.position == path[targetIndex])
            {
                targetIndex++;
                RequestNewPath();
                return;
            }

            Vector3 direction = (path[targetIndex] - transform.position).normalized;
            Vector3 movePosition = transform.position + (direction * speed * Time.fixedDeltaTime);

            transform.LookAt(movePosition);

            if (Vector3.Distance(transform.position, path[targetIndex]) < Vector3.Distance(transform.position, movePosition))
            {
                transform.position = path[targetIndex];
                return;
            }

            transform.position = movePosition;
        }
    }

    void RequestNewPath()
    {
        float heightDifference = transform.position.y - m_player.transform.position.y;
        Vector3 offset = new Vector3(0, heightDifference, 0);

        startId = cloudNav.aStar.GetClosestPoint(transform.position);
        endId = cloudNav.aStar.GetClosestPoint(m_player.transform.position - offset);

        cloudNav.aStar.RequestPath(GetNewPath, startId, endId);
    }

    void GetNewPath(List<Vector3> _path)
    {
        path.Clear();
        targetIndex = 0;
        path = _path;
    }

    public void Stun()
    {
        stopped = true;
        recoveringFromStun = false;

        //grace period item
        m_stunTimer = stunTime + EnemyStunModifier.extraStunTime;
        stunned = true;

        Debug.Log($"[FlyingEnemy] STUN APPLIED | base={stunTime:F2}, bonus={EnemyStunModifier.extraStunTime:F2}, total={m_stunTimer:F2}");
    }

    public override void Attack()
    {
        if (m_curFireTime > attackSpeed * 0.5f)
            transform.LookAt(m_player.transform);

        m_curFireTime -= Time.fixedDeltaTime;
        if (m_curFireTime <= 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
            bullet.GetComponent<Bullet>().damage = baseDamage;
            m_curFireTime = attackSpeed;
        }
    }
}