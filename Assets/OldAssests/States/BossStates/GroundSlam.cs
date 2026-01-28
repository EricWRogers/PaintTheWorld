using SuperPupSystems.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class GroundSlam : SimpleState
{

    [Header("State Machine Gets These")]
    public SimpleStateMachine sm;
    public Animator anim;
    public Transform target;
    public GameObject boss;
    public NavMeshAgent agent;

    private Boss m_boss;

    public override void OnStart()
    {
        m_boss = boss.GetComponent<Boss>();
        agent.enabled = false;
        Debug.Log("GroundSlam");

    }
    public override void UpdateState(float dt)
    {
        if (m_boss.canSlam)
        {
            m_boss.SlamAttack();
        }
        if (m_boss.slamIsDone)
        {
            sm.ChangeState(nameof(WalkToPlayer));
        }
        
    }
    public override void OnExit()
    {
        base.OnExit();
        agent.enabled = true;
    }
    
}
