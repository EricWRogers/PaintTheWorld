using UnityEngine;
using SuperPupSystems.Helper;
using KinematicCharacterControler;
using UnityEngine.AI;
//using DG.Tweening;

public class Mortar : Enemy
{
    [Header("Mortar Settings")]
    public float aimDelay = 1.5f;
    public float flightTime = 2f;
    public float fireCooldown = 4f;
    public float extraHeight;
    public LayerMask groundMask;
    public LayerMask wallMask;
    public float hitRadius;

    [Header("Indicator Settings")]
    public GameObject targetIndicatorPrefab;
    public Vector3 offset;

    private float m_attackTimer = 0f;
    private Vector3 m_targetPos;
    private GameObject m_currentIndicator;

    [HideInInspector] public bool hasTarget;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound;
    [Range(0f, 1f)]
    public float fireSoundVolume = 0.9f;
    [Range(0.95f, 1.05f)]
    public float randomPitchRange = 1.02f;

    [Header("Movement")]
    public LayerMask losMask;
    private RaycastHit m_hitInfo;
    private NavMeshAgent m_agent;
    public Transform m_targetTransform;
    private Vector3 m_direction;

    new void Start()
    {
        base.Start();
        m_agent = GetComponent<NavMeshAgent>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
        }
        Move();
    }

    new void Update()
    {
        base.Update();
        m_direction = PlayerManager.instance.player.transform.position - transform.position;
        //if has los of player shoot
        if(Physics.Raycast(transform.position, m_direction, out m_hitInfo, 100, losMask))
        {
            Debug.Log(m_hitInfo.transform.gameObject.name);
            if(m_hitInfo.transform.CompareTag("Player"))
            {
                m_attackTimer -= Time.deltaTime;
                StopMoving();
                if(m_attackTimer <= 0)
                {
                    Attack();
                    m_attackTimer = fireCooldown;
                }
                if(!hasTarget)
                {
                    
                    m_direction.y = 0;

                    if (m_direction == Vector3.zero)
                        return;

                    m_direction = Vector3.Normalize(m_direction);

                    float angle = Mathf.Atan2(m_direction.x, m_direction.z) * Mathf.Rad2Deg;
                    transform.localEulerAngles = new Vector3(0, angle, 0);
                }
            }
            else
            {
                m_attackTimer = fireCooldown;
                Move();
            }
        }
    }

    public override void Attack()
    {
        if (!hasTarget)
        {
            m_targetPos = GetPlayerTargetPos();
            hasTarget = true;

            ShowTargetIndicator(m_targetPos + offset);
            Invoke(nameof(FireShell), aimDelay);
        }
    }

    private void FireShell()
    {
        if (bulletPrefab == null || firePoint == null) return;

        if (audioSource != null && fireSound != null)
        {
            float delta = randomPitchRange - 1f;
            float pitch = Random.Range(1f - delta, 1f + delta);
            audioSource.pitch = pitch;
            audioSource.PlayOneShot(fireSound, fireSoundVolume);
        }

        MortarShell shell = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity).GetComponent<MortarShell>();
        shell.mortar = this;
        shell.Launch(firePoint.position, m_targetPos, flightTime, extraHeight, hitRadius);
        
    }

    private Vector3 GetPlayerTargetPos()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        RaycastHit hit;

        if (playerMovement.leftWall)
        {
            if (Physics.Raycast(player.transform.position, -player.transform.right, out hit, 5f, wallMask))
                return hit.point;
        }
        else if (playerMovement.rightWall)
        {
            if (Physics.Raycast(player.transform.position, player.transform.right, out hit, 5f, wallMask))
                return hit.point;
        }

        if (Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, out hit, 100f, groundMask))
            return hit.point;

        return player.transform.position;
    }

    
    private void ShowTargetIndicator(Vector3 position)
    {
        if (targetIndicatorPrefab == null) return;
        m_currentIndicator = Instantiate(targetIndicatorPrefab, position, Quaternion.identity, transform);
        m_currentIndicator.transform.localScale = Vector3.zero;
       // m_currentIndicator.transform.DOScale(new Vector3(hitRadius, hitRadius, hitRadius), aimDelay);
        Destroy(m_currentIndicator, aimDelay + flightTime);
    }

    public void Move()
    {
        m_agent.SetDestination(m_targetTransform.position);
    }

    public void StopMoving()
    {
        m_agent.SetDestination(transform.position);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawRay(transform.position, m_direction);
    }
}
