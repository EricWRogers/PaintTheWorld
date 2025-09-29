using System.Collections.Generic;
using UnityEngine;

public class AttackZone : MonoBehaviour
{
    public bool enemyInZone;
    public List<GameObject> enemiesInZone = new List<GameObject>();
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyInZone = true;
        }
        else
        {
            enemyInZone = false;
        }
        if (!enemiesInZone.Contains(other.gameObject) && other.CompareTag("Enemy"))
        {
            enemiesInZone.Add(other.gameObject);
        }

    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyInZone = false;
        }
        if (enemiesInZone.Contains(other.gameObject) && other.CompareTag("Enemy"))
        {
            enemiesInZone.Remove(other.gameObject);
        }
    }
}
