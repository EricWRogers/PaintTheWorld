using SuperPupSystems.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class MeleeAttack : SimpleState
{
    public UnityEvent attackEvent;

    [Header("State Machine Gets These")]
    public Animator anim;
    public GameObject enemyObj;
    public float attackRange;
    public Transform target;
    public SimpleStateMachine sm;

    private Enemy m_enemy;
    public bool m_isRotating = false;
    public override void OnStart()
    {
        m_enemy = enemyObj.GetComponent<Enemy>();
        
        
    }
    public override void UpdateState(float dt)
    {
        Vector3 dir = target.position - enemyObj.transform.position;
        float angle = Vector3.Angle(enemyObj.transform.forward, dir);
        if (angle > 45f && !m_isRotating)
        {
            m_isRotating = true;
            anim.SetBool("AttackAnim", false);
        }
        if (!m_isRotating)
        {
            anim.SetBool("AttackAnim", true);
            attackEvent.Invoke();
        }
        if (Vector3.Distance(enemyObj.transform.position, target.position) >= attackRange)
        {
            anim.SetBool("AttackAnim", false);

            if (!enemyObj.GetComponent<Enemy>().inAttackAnim)
                sm.ChangeState(nameof(MoveToPlayer));
        }
        if (m_isRotating)
        {
            if (!enemyObj.GetComponent<Enemy>().inAttackAnim)
            {
                Quaternion lookRotation = Quaternion.LookRotation(dir);
                enemyObj.transform.rotation = Quaternion.Slerp(enemyObj.transform.rotation, lookRotation, Time.deltaTime * m_enemy.rotationSpeed);
                if (angle < 10)
                {
                    m_isRotating = false;
                }
            }
        }
    }
    public override void OnExit()
    {
        base.OnExit();
        enemyObj.GetComponent<NavMeshAgent>().SetDestination(m_enemy.player.transform.position);

    }
}
