using System.Collections.Generic;
using UnityEngine;

public class AttackZone : MonoBehaviour
{
 public bool enemyInZone;
    public List<GameObject> enemiesInZone = new List<GameObject>();

    // No Start() needed

    void Update()
    {
        // NEW: This line cleans the list of any enemies that were destroyed
        // This prevents "MissingReferenceException" errors
        enemiesInZone.RemoveAll(item => item == null);

        // NEW: The flag is now always based on the actual count of enemies
        enemyInZone = enemiesInZone.Count > 0;
    }

    void OnTriggerEnter(Collider other)
    {
        // If an enemy enters and isn't already in our list, add it.
        if (other.CompareTag("Enemy") && !enemiesInZone.Contains(other.gameObject))
        {
            enemiesInZone.Add(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        // If an enemy leaves, remove it from the list.
        if (other.CompareTag("Enemy") && enemiesInZone.Contains(other.gameObject))
        {
            enemiesInZone.Remove(other.gameObject);
        }
    }
}
