using UnityEngine;
using SuperPupSystems.StateMachine;
using UnityEngine.AI;

[System.Serializable]
public class MoveToPlayer : SimpleState
{
    public NavMeshAgent agent;
    public GameObject enemy;
    public float attackRange;
    public Transform target;
    public SimpleStateMachine sm;
    public override void OnStart()
    {
        agent.SetDestination(target.position);
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


    }

}
