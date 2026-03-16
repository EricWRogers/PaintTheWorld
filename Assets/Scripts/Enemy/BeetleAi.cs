using UnityEngine;
using UnityEngine.AI;
using SuperPupSystems.Helper;
public class BeetleAi : Enemy
{
    private float m_attackTimer;
    public bool stunned;
    public float stunTime;
    private float m_stunTimer;
    public float spreadAngleForObj;

    [Header("Movement")]
    public LayerMask losMask;
    private RaycastHit m_hitInfo;
    private NavMeshAgent m_agent;
    private Vector3 m_direction;
    public float repathDistanceThreshold = 1.0f;
    private Vector3 m_lastTargetPosition;
    

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    [Range(0f, 1f)]
    public float fireSoundVolume = 0.9f;
    [Range(0.95f, 1.05f)]
    public float randomPitchRange = 1.02f;
    new void Start()
    {
        base.Start();
        
    }

    // Update is called once per frame
    new void Update()
    {
        if(m_agent == null)
        {
            m_agent = GetComponent<NavMeshAgent>();
            return;
        }
        if (stunned)
        {
            m_stunTimer -= Time.deltaTime;
            if(m_stunTimer <= 0)
            {
                modelMeshRenderer.materials[1].color = Color.clear;
                Move();
                GetComponent<Health>().Revive();
                stunned = false;
                GameEvents.EnemyRecoveredFromStun?.Invoke(gameObject);
            }
            return;
        }
        base.Update();

        if (Vector3.Distance(target.position, m_lastTargetPosition) > repathDistanceThreshold)
        {
            m_lastTargetPosition = target.position;
            Move();
        }

        if (CheckForTarget())
        {
            m_direction = Vector3.Normalize(m_direction);
            float angle = Mathf.Atan2(m_direction.x, m_direction.z) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0, angle, 0);
            Attack();
        }
        
    }

    public bool CheckForTarget()
    {
        m_direction = target.transform.position - transform.position;
        
        if(targetingPlayer)
        {
            if(Physics.Raycast(transform.position, m_direction, out m_hitInfo, attackRange, losMask))
            {
                return m_hitInfo.transform.CompareTag("Player");
            }
            else
                return false;
        }
        else
        {
            if(Physics.Raycast(transform.position, m_direction, out m_hitInfo, attackRange, losMask))
            {
                return m_hitInfo.transform.gameObject.GetComponent<PaintingObj>();
            }
            else
                return false;
        }

    }

    public override void Attack()
    {
        StopMoving();
        m_attackTimer -= Time.deltaTime;

        if (m_attackTimer > 0) return;
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
        m_attackTimer = attackSpeed;
    }

    public void Stun()
    {
        StopMoving();
        modelMeshRenderer.materials[1].color = stunColor;
        m_stunTimer = stunTime;
        stunned = true;
    }

    public void Move()
    {
        m_agent.SetDestination(target.position);
    }

    public void StopMoving()
    {
        m_agent.SetDestination(transform.position);
    }

}
