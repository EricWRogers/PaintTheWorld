using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;
using SuperPupSystems.Helper;
public class BeetleAi : Enemy
{
    private float m_attackTimer;
    public bool stunned;
    public float stunTime;
    private float m_stunTimer;
    public float spreadAngleForObj;
    private float m_recoveryGraceTimer;
    private bool recoveringFromStun;
    public float distanceFromPlayerToTarget = 20f;
    public Transform rearViewPoint;

    [Header("Movement")]
    public LayerMask losMask;
    private RaycastHit m_hitInfo1;
    private RaycastHit m_hitInfo2;
    private NavMeshAgent m_agent;
    private Vector3 m_direction;
    public float repathDistanceThreshold = 1.0f;
    private Vector3 m_lastTargetPosition;
    public float attackAngleThreshold = 5f;
    

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    [Range(0f, 1f)]
    public float fireSoundVolume = 0.9f;
    [Range(0.95f, 1.05f)]
    public float randomPitchRange = 1.02f;

    [Header("Particle Effects")]
    public ParticleSystem particles, particles2;
    public List<ParticleSystem> particleSystems = new List<ParticleSystem>();
    new void Start()
    {
        base.Start();
        anim = GetComponent<Animator>();
        anim.SetBool("Moving", true);
        
        if (particles != null)
        {
            var main = particles.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            particles.Stop();
        }

        if (particles2 != null)
        {
            var main = particles2.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            particles2.Stop();
        }

        foreach (var ps in particleSystems)
        {
            var main = ps.main;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            ps.Stop();
        }
    }

    // Update is called once per frame
    new void Update()
    {

        base.Update();
        if(m_agent == null)
        {
            m_agent = GetComponent<NavMeshAgent>();
            return;
        }
        if(target == null)
        {
            return;
        }



        if (Vector3.Distance(transform.position, PlayerManager.instance.player.transform.position) <= distanceFromPlayerToTarget)
        {
            if(target.GetComponent<PaintingObj>() && target.GetComponent<PaintingObj>().currentEnemiesTarget > 0)
                target.GetComponent<PaintingObj>().currentEnemiesTarget--;

            target = PlayerManager.instance.player.transform;
            targetingPlayer = true;
        }
        else
        {
            targetingPlayer = false;

            // float maxDistance = 1000;
            // int index = 0;
            // for(int i = 0; i < GameManager.instance.activeObjectives.Count - 1; i++)
            // {
            //     float distance = Vector3.Distance(transform.position, GameManager.instance.activeObjectives[i].transform.position);
            //     if(distance <= maxDistance)
            //     {
            //         index = i;
            //         maxDistance = distance;
            //     }
            // }
            if(!target.GetComponent<PaintingObj>())
                target = EnemyManager.instance.GetObjectiveTarget().transform;
        }
        
        if (stunned)
        {
            m_stunTimer -= Time.deltaTime;
            Debug.Log($"[BeetleAi] STUNNED | timer={m_stunTimer:F2}");

            if (m_stunTimer <= 0)
            {
                GetComponent<Health>().Revive();
                stunned = false;

                anim.SetTrigger("Unstun");

                recoveringFromStun = true;
                m_recoveryGraceTimer = EnemyStunModifier.extraRecoveryGrace;

                Debug.Log($"[BeetleAi] Recovered from stun, entering grace for {m_recoveryGraceTimer:F2}s");

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
                Debug.Log("[BeetleAi] Grace period ended, resuming movement.");
            }
            return;
        }

        if (Vector3.Distance(target.position, m_lastTargetPosition) > repathDistanceThreshold)
        {
            m_lastTargetPosition = target.position;
            Move();
        }
        
        if(Vector3.Distance(transform.position, target.transform.position) <= attackRange)
        {
            m_direction = (target.position - transform.position).normalized;
            if(Physics.Raycast(transform.position, m_direction, out m_hitInfo1, attackRange, losMask) && Physics.Raycast(rearViewPoint.position, m_direction, out m_hitInfo2, attackRange, losMask))
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
                    if (m_hitInfo1.transform.gameObject.GetComponent<PaintingObj>() && m_hitInfo2.transform.gameObject.GetComponent<PaintingObj>())
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

    public override void Attack()
    {
        StopMoving();
        anim.SetBool("Attacking", true);

        // m_attackTimer -= Time.deltaTime;

        // if (m_attackTimer > 0) return;
        
        // m_attackTimer = attackSpeed;
    }

    public void Stun()
    {
        StopMoving();
        anim.SetTrigger("Stun");
        m_stunTimer = stunTime + EnemyStunModifier.extraStunTime;
        stunned = true;
        recoveringFromStun = false;
        PlayStunEffect();
        Debug.Log($"[BeetleAi] STUN APPLIED | base={stunTime:F2}, bonus={EnemyStunModifier.extraStunTime:F2}, total={m_stunTimer:F2}");
    }

    public void Move()
    {
        anim.SetBool("Moving", true);
        m_agent.SetDestination(target.position);
        StopSprayEffect();
    }

    public void StopMoving()
    {
        anim.SetBool("Moving", false);
        m_agent.SetDestination(transform.position);
    }
    void Fire()
    {
        Debug.Log("fire");
        if (bulletPrefab == null || firePoint == null) return;

        firePoint.transform.LookAt(target);

        if (!targetingPlayer)
        {
            float halfAngle = spreadAngleForObj * 0.5f;
            float yaw = Random.Range(-halfAngle, halfAngle);
            float pitch = Random.Range(-halfAngle, halfAngle);

            firePoint.transform.rotation *= Quaternion.Euler(pitch, yaw, 0f);
        }

        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        StopSprayEffect();
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

    private void StartSprayEffect()
    {
        if (particles != null) particles.Play();
        if (particles2 != null) particles2.Play();
    }

    private void PlayStunEffect()
    {
        foreach (var ps in particleSystems)
        {
            ps.Play();
        }
    }

    private void StopStunEffect()
    {
        foreach (var ps in particleSystems)
        {
            ps.Stop();
        }
    }

    private void StopSprayEffect()
    {
        if (particles != null) particles.Stop();
        if (particles2 != null) particles2.Stop();
    }
}
