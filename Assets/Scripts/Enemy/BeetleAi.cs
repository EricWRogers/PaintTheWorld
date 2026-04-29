using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class BeetleAi : Enemy
{

    [Header("State")]
    public bool stunned;
    public float stunTime;
    private float m_stunTimer;
    private float m_recoveryGraceTimer;
    private bool recoveringFromStun;

    [Header("Movement")]
    public LayerMask losMask;
    private RaycastHit m_hitInfo1;
    private RaycastHit m_hitInfo2;
    private NavMeshAgent m_agent;
    private Vector3 m_direction;
    public float repathDistanceThreshold = 1.0f;
    private Vector3 m_lastTargetPosition;
    public float attackAngleThreshold = 5f;

    [Header("Patrol")]
    private int patrolIndex = 0;

    [Header("Combat")]
    public float spreadAngleForObj;
    public Transform rearViewPoint;
    public float distanceToPushPlayerBack;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    [Range(0f, 1f)] public float fireSoundVolume = 0.9f;
    [Range(0.95f, 1.05f)] public float randomPitchRange = 1.02f;

    [Header("Particle Effects")]
    public ParticleSystem particles, particles2;
    public List<ParticleSystem> particleSystems = new List<ParticleSystem>();

    new void Start()
    {
        base.Start();

        anim = GetComponent<Animator>();
        anim.SetBool("Moving", true);

        m_agent = GetComponent<NavMeshAgent>();

        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.Stop();
        }
    }

    new void Update()
    {
        base.Update();

        if (m_agent == null || PlayerManager.instance.player == null)
            return;

        float playerDist = Vector3.Distance(transform.position, PlayerManager.instance.player.transform.position);

        bool shouldTargetPlayer = Physics.Raycast(transform.position, m_direction, out m_hitInfo1, targetDistance, losMask);

        bool shouldTargetObjective = false;
        if (patroling != null && patroling.paintingObj != null)
        {
            shouldTargetObjective = patroling.paintingObj.percentageCovered > 0;
        }

        if (shouldTargetPlayer)
        {
            if (target != PlayerManager.instance.player.transform)
            {
                if (target != null && target.GetComponent<PaintingObj>())
                    target.GetComponent<PaintingObj>().currentEnemiesTarget--;

                target = PlayerManager.instance.player.transform;
            }

            targetingPlayer = true;
        }
        else if (shouldTargetObjective)
        {
            if (patroling != null && patroling.paintingObj != null)
            {
                target = patroling.paintingObj.transform;
            }

            targetingPlayer = false;
        }
        else
        {
            targetingPlayer = false;
            Patrol();
            return;
        }
        if (playerDist <= distanceToPushPlayerBack)
        {
            Vector3 dir = (PlayerManager.instance.player.transform.position - transform.position).normalized;
            PlayerManager.instance.player.GetComponent<PlayerMovement>().AddForce(dir);
        }

        if (stunned)
        {
            m_stunTimer -= Time.deltaTime;

            if (m_stunTimer <= 0)
            {
                GetComponent<Health>().Revive();
                stunned = false;

                anim.SetTrigger("Unstun");

                recoveringFromStun = true;
                m_recoveryGraceTimer = EnemyStunModifier.extraRecoveryGrace;

                GameEvents.EnemyRecoveredFromStun?.Invoke(gameObject);
            }
            return;
        }

        if (recoveringFromStun)
        {
            m_recoveryGraceTimer -= Time.deltaTime;

            if (m_recoveryGraceTimer <= 0f)
            {
                recoveringFromStun = false;
                Move();
                StopStunEffect();
            }
            return;
        }

        if (Vector3.Distance(target.position, m_lastTargetPosition) > repathDistanceThreshold)
        {
            m_lastTargetPosition = target.position;
            Move();
        }

        if (Vector3.Distance(transform.position, target.position) <= attackRange)
        {
            m_direction = (target.position - transform.position).normalized;

            if (Physics.Raycast(transform.position, m_direction, out m_hitInfo1, attackRange, losMask) &&
                Physics.Raycast(rearViewPoint.position, m_direction, out m_hitInfo2, attackRange, losMask))
            {
                if (targetingPlayer)
                {
                    if (m_hitInfo1.transform.CompareTag("Player"))
                    {
                        StopMoving();
                        if (RotateTowardsTarget())
                        {
                            Attack();
                        }
                    }
                }
                else
                {
                    if (m_hitInfo1.transform.GetComponent<PaintingObj>() &&
                        m_hitInfo2.transform.GetComponent<PaintingObj>())
                    {
                        StopMoving();
                        if (RotateTowardsTarget())
                        {
                            Attack();
                        }
                    }
                }
            }
        }
    }

    void Patrol()
    {
        if (patroling == null || patroling.patrolPoints.Count == 0)
            return;

        Transform patrolTarget = patroling.patrolPoints[patrolIndex];

        if (Vector3.Distance(transform.position, patrolTarget.position) < patrolingDistanceCheck)
        {
            patrolIndex = (patrolIndex + 1) % patroling.patrolPoints.Count;
        }

        m_agent.SetDestination(patrolTarget.position);
    }

    public override void Attack()
    {
        StopMoving();
        anim.SetBool("Attacking", true);
    }

    public void Move()
    {
        anim.SetBool("Moving", true);
        m_agent.SetDestination(target.position);
    }

    public void StopMoving()
    {
        anim.SetBool("Moving", false);
        m_agent.SetDestination(transform.position);
    }

    bool RotateTowardsTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        if (direction == Vector3.zero) return true;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        return angle <= attackAngleThreshold;
    }

    public void Stun()
    {
        StopMoving();
        anim.SetTrigger("Stun");
        m_stunTimer = stunTime + EnemyStunModifier.extraStunTime;
        stunned = true;
        recoveringFromStun = false;
        PlayStunEffect();
    }

    void Fire() 
    { 
        Debug.Log("fire"); 
        if (bulletPrefab == null || firePoint == null) return; 
        firePoint.transform.LookAt(target); 
        if (!targetingPlayer) 
        {
            float halfAngle = spreadAngleForObj * 0.5f; float yaw = Random.Range(-halfAngle, halfAngle); 
            float pitch = Random.Range(-halfAngle, halfAngle); firePoint.transform.rotation *= Quaternion.Euler(pitch, yaw, 0f); 
        } 
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        StopSprayEffect(); 
    } 
    private void StartSprayEffect() 
    {
        if (particles != null) particles.Play(); 
        if (particles2 != null) particles2.Play(); 
    }

    private void PlayStunEffect()
    {
        foreach (var ps in particleSystems)
            ps.Play();
    }

    private void StopStunEffect()
    {
        foreach (var ps in particleSystems)
            ps.Stop();
    }
    private void StopSprayEffect() 
    { 
        if (particles != null) particles.Stop(); 
        if (particles2 != null) particles2.Stop(); 
    }
}