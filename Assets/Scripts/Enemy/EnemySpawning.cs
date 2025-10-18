using SuperPupSystems.Helper;
using UnityEngine;

public class EnemySpawning : MonoBehaviour
{
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 50f);
    public LayerMask layerToCheckForSpawning;
    public GameObject indicator;

    public void SpawnEnemy(string _enemy)
    {
        Vector3 spawnPosition = new Vector3
        (
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        ) + transform.position;
        if (IsSpawnPointClear(spawnPosition, 1.5f, layerToCheckForSpawning))
        {
            SimpleObjectPool.instance.SpawnFromPool(_enemy, spawnPosition, Quaternion.identity);
        }
            
    }

    private bool IsSpawnPointClear(Vector3 spawnPosition, float radius, LayerMask obstacleLayer)
    {
        Collider[] colliders = Physics.OverlapSphere(spawnPosition, radius, obstacleLayer);
        return colliders.Length == 0;
    }

    void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}
