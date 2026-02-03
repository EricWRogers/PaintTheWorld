// using SuperPupSystems.Helper;
// using SuperPupSystems.StateMachine;
// using UnityEngine;
// using UnityEngine.AI;
// using UnityEngine.Events;

// [System.Serializable]
// public class WalkToPlayer : SimpleState
// {
    
//     [Header("State Machine Gets These")]
//     public SimpleStateMachine sm;
//     public Transform target;
//     public GameObject boss;
//     public NavMeshAgent agent;

//     private Boss m_boss;
//     private float m_curWalkTime;

//     public override void OnStart()
//     {
//         m_boss = boss.GetComponent<Boss>();
//         agent.SetDestination(target.position);
//         agent.isStopped = false;
//         m_curWalkTime = m_boss.walkTime;
//         Debug.Log("WalkToPlayer");
//     }
//     public override void UpdateState(float dt)
//     {
        
//         agent.SetDestination(target.position);
//         if (m_boss.canLaser)
//         {
//             m_boss.LaserBeam();
//         }
//         if (m_boss.isLasering)
//         {
//             agent.isStopped = true;
//         }
//         else
//         {
//             agent.isStopped = false;
//             m_curWalkTime -= Time.fixedDeltaTime;
//         }
//         if (m_curWalkTime < 0)
//         {
//             sm.ChangeState(nameof(SwingArms));
//         }


//     }
//     public override void OnExit()
//     {
//         base.OnExit();
//         agent.isStopped = true;

//     }

    
    
// }
