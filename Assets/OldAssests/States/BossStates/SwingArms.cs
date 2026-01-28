using SuperPupSystems.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class SwingArms : SimpleState
{
    [Header("State Machine Gets These")]
    public SimpleStateMachine sm;
    public Animator anim;
    public Transform target;
    public GameObject boss;
    public NavMeshAgent agent;

    private Boss m_boss;
    private bool hasSwung;
    public override void OnStart()
    {
        m_boss = boss.GetComponent<Boss>();
        Debug.Log("SwingArms");
        hasSwung = false;

        if (m_boss.canSwing)
        {
            anim.SetBool("Swing", true);
        }
    }
    public override void UpdateState(float dt)
    {
        if (!anim.GetBool("Swing"))
        {
            sm.ChangeState(nameof(GroundSlam));
        }
    }
    public override void OnExit()
    {
        base.OnExit();

    }
}
