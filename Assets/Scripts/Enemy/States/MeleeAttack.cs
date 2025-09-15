using SuperPupSystems.StateMachine;
using UnityEngine;

[System.Serializable]
public class MeleeAttack : SimpleState
{
    public Animator anim;
    public GameObject enemy;
    public float attackRange;
    public Transform target;
    public SimpleStateMachine sm;
    public override void OnStart()
    {

    }
    public override void UpdateState(float dt)
    {
        anim.SetBool("AttackAnim", true);

        if (Vector3.Distance(enemy.transform.position, target.position) >= attackRange)
        {
            anim.SetBool("AttackAnim", false);
            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !anim.IsInTransition(0))
                sm.ChangeState(nameof(MoveToPlayer));
        }
    }
    public override void OnExit()
    {
        base.OnExit();


    }
}
