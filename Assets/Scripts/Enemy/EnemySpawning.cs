using SuperPupSystems.Helper;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 50f);
    public LayerMask layerToCheckForSpawning;

    public GameObject SpawnEnemy(string _enemy)
    {
        Vector3 spawnPosition = new Vector3
        (
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        ) + transform.position;
        GameObject temp =  SimpleObjectPool.instance.SpawnFromPool(_enemy, spawnPosition, Quaternion.identity);
        return temp;
    }

    void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}
