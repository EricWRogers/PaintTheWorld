using UnityEngine;
using UnityEngine.AI;

public class CleanBot : MonoBehaviour
{
    [Header("References")]
    public Transform player;

    [Header("Movement")]
    public float fleeRadius = 8f;
    public float fleeDistance = 10f;
    public float wanderRadius = 6f;
    public float wanderInterval = 2f;

    [Header("Cleaning")]
    public float cleanRadius = 1.2f;
    public float cleanInterval = 0.2f;
    public float raycastHeight = 1f;
    public float raycastDistance = 5f;

    private NavMeshAgent m_agent;
    private float m_wanderTimer;
    private float m_cleanTimer;

    void Start()
    {
        m_agent = GetComponent<NavMeshAgent>();

        if (player == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            player = PlayerManager.instance.player.transform;
        }

        PickWanderPoint();
    }

    void Update()
    {
        if (player == null && PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            player = PlayerManager.instance.player.transform;
        }

        HandleMovement();
        HandleCleaning();
    }

    void HandleMovement()
    {
        if (player == null || m_agent == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= fleeRadius)
        {
            FleeFromPlayer();
            m_wanderTimer = 0f;
        }
        else
        {
            m_wanderTimer -= Time.deltaTime;

            if (m_wanderTimer <= 0f || !m_agent.hasPath || m_agent.remainingDistance <= m_agent.stoppingDistance + 0.2f)
            {
                PickWanderPoint();
                m_wanderTimer = wanderInterval;
            }
        }
    }

    void HandleCleaning()
    {
        m_cleanTimer -= Time.deltaTime;

        if (m_cleanTimer > 0f)
            return;

        m_cleanTimer = cleanInterval;

        Vector3 rayStart = transform.position + Vector3.up * raycastHeight;

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance))
        {
            Paintable paintable = hit.collider.GetComponent<Paintable>();

            if (paintable == null)
            {
                paintable = hit.collider.GetComponentInParent<Paintable>();
            }

            if (paintable != null)
            {
                PaintManager.instance.paint(
                    paintable,
                    hit.point,
                    cleanRadius,
                    1f,
                    1f,
                    Color.clear
                );
            }
        }
    }

    void FleeFromPlayer()
    {
        Vector3 awayDirection = (transform.position - player.position).normalized;
        Vector3 desiredPoint = transform.position + awayDirection * fleeDistance;

        if (NavMesh.SamplePosition(desiredPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            m_agent.SetDestination(hit.position);
        }
    }

    void PickWanderPoint()
    {
        Vector3 randomOffset = new Vector3(
            Random.Range(-wanderRadius, wanderRadius),
            0f,
            Random.Range(-wanderRadius, wanderRadius)
        );

        Vector3 desiredPoint = transform.position + randomOffset;

        if (NavMesh.SamplePosition(desiredPoint, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            m_agent.SetDestination(hit.position);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, cleanRadius);

        Vector3 rayStart = transform.position + Vector3.up * raycastHeight;
        Gizmos.color = Color.green;
        Gizmos.DrawLine(rayStart, rayStart + Vector3.down * raycastDistance);
    }
}