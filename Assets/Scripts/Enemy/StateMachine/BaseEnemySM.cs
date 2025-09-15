using UnityEngine;
using SuperPupSystems.StateMachine;
using UnityEngine.AI;
public class BaseEnemySM : SimpleStateMachine
{
    private GameObject m_enemy;

    [Header("States")]
    public MoveToPlayer moveToPlayer;
    public MeleeAttack meleeAttack;

    private void Awake()
    {
        states.Add(moveToPlayer);
        states.Add(meleeAttack);

        foreach (SimpleState s in states)
            s.stateMachine = this;

        m_enemy = GameObject.FindGameObjectWithTag("Enemy");

        //sets moveplayer varibles
        moveToPlayer.enemy = m_enemy;
        moveToPlayer.agent = m_enemy.GetComponent<NavMeshAgent>();
        moveToPlayer.target = m_enemy.GetComponent<Enemy>().player.transform;
        moveToPlayer.attackRange = m_enemy.GetComponent<Enemy>().attackRange;
        moveToPlayer.sm = this;

        //sets meleeAttack varibles
        meleeAttack.anim = m_enemy.GetComponent<Enemy>().anim;
        meleeAttack.enemyObj = m_enemy;
        meleeAttack.target = m_enemy.GetComponent<Enemy>().player.transform;
        meleeAttack.attackRange = m_enemy.GetComponent<Enemy>().attackRange;
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
