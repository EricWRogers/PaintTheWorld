using UnityEngine;
using UnityEngine.AI;

public class AIGroundMovement : MonoBehaviour
{
    private NavMeshAgent m_agent;
    private GameObject m_target;
    public void Move()
    {
        m_agent.SetDestination(m_target.transform.position);
    }

    public void StopMoving()
    {
        m_agent.SetDestination(transform.position);
    }
}
