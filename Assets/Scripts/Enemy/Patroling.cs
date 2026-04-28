using System.Collections.Generic;
using UnityEngine;

public class Patroling : MonoBehaviour
{
    public List<Transform> patrolPoints;
    public int numEnemyPatrolling;
    public bool flyingPatrol;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(patrolPoints.Count != transform.childCount)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                patrolPoints.Add(transform.GetChild(i));
            }
        }

    }
}
