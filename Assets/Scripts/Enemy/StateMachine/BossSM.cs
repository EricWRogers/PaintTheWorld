using UnityEngine;
using SuperPupSystems.StateMachine;
using UnityEngine.AI;

public class BossSM : SimpleStateMachine
{
    [Header("States")]
    public WalkToPlayer walkToPlayer;
    public SwingArms swingArms;
    public GroundSlam groundSlam;

    

    private void Awake()
    {
        states.Add(walkToPlayer);
        states.Add(swingArms);
        states.Add(groundSlam);

        foreach (SimpleState s in states)
            s.stateMachine = this;
    }

    void Start()
    {
        //walk to player varibles
        walkToPlayer.sm = this;
        walkToPlayer.target = gameObject.GetComponent<Boss>().player.transform;
        walkToPlayer.boss = gameObject;
        walkToPlayer.agent = gameObject.GetComponent<NavMeshAgent>();

        //swing arms varibles
        swingArms.sm = this;
        swingArms.target = gameObject.GetComponent<Boss>().player.transform;
        swingArms.boss = gameObject;
        swingArms.agent = gameObject.GetComponent<NavMeshAgent>();
        //swingArms.anim = gameObject.GetComponent<Boss>().anim;

        //ground slam varibles
        groundSlam.sm = this;
        groundSlam.target = gameObject.GetComponent<Boss>().player.transform;
        groundSlam.boss = gameObject;
        groundSlam.agent = gameObject.GetComponent<NavMeshAgent>();
        //groundSlam.anim = gameObject.GetComponent<Boss>().anim;


        ChangeState(nameof(WalkToPlayer));
    }
}
