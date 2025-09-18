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
    }
    public override void UpdateState(float dt)
    {

        if (m_boss.canSlam)
        {
            //do slam
        }
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}
