using UnityEngine;
using SuperPupSystems.StateMachine;
using UnityEngine.AI;
public class BaseEnemySM : SimpleStateMachine
{

    [Header("States")]
    public MoveToPlayer moveToPlayer;
    public MeleeAttack meleeAttack;

    private void Awake()
    {
        states.Add(moveToPlayer);
        states.Add(meleeAttack);

        foreach (SimpleState s in states)
            s.stateMachine = this;


        //sets moveplayer varibles
        moveToPlayer.enemy = gameObject;
        moveToPlayer.agent = gameObject.GetComponent<NavMeshAgent>();
        moveToPlayer.target = gameObject.GetComponent<Enemy>().player.transform;
        moveToPlayer.attackRange = gameObject.GetComponent<Enemy>().attackRange;
        moveToPlayer.sm = this;

        //sets meleeAttack varibles
        meleeAttack.anim = gameObject.GetComponent<Enemy>().anim;
        meleeAttack.enemyObj = gameObject;
        meleeAttack.target = gameObject.GetComponent<Enemy>().player.transform;
        meleeAttack.attackRange = gameObject.GetComponent<Enemy>().attackRange;
        meleeAttack.sm = this;
    }

    void Start()
    {
        ChangeState(nameof(MoveToPlayer));

    }

    void Update()
    {
        
    }
}
