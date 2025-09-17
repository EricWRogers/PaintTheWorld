using UnityEngine;
using SuperPupSystems.StateMachine;
using UnityEngine.AI;

[System.Serializable]
public class MoveToPlayer : SimpleState
{
    [Header("State Machine Gets These")]
    public NavMeshAgent agent;
    public GameObject enemy;
    public float attackRange;
    public Transform target;
    public SimpleStateMachine sm;
    public override void OnStart()
    {
        agent.SetDestination(target.position);
        agent.isStopped = false;
    }
    public override void UpdateState(float dt)
    {
        agent.SetDestination(target.position);
        if (Vector3.Distance(enemy.transform.position, target.position) <= attackRange)
        {
            sm.ChangeState(nameof(MeleeAttack));
        }
    }
    public override void OnExit()
    {
        base.OnExit();
        agent.isStopped = true;

    }

}
