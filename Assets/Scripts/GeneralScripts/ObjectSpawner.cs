using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 50f);
    public int totalSpawned = 0;
    public int poolSize = 20;
    public List<GameObject> objectsToSpawn;
    public bool spawnAllOnStart;

    void Start()
    {
        if (spawnAllOnStart)
        {
            SpawnMax();
        }
    }

    public void SpawnObject()
    {
        int ranInt = Random.Range(0, objectsToSpawn.Count);
        Vector3 spawnPosition = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        ) + transform.position;

        GameObject.Instantiate(objectsToSpawn[ranInt], spawnPosition, transform.rotation);
    }

    public void SpawnMax()
    {
        for (int i = 0; i < poolSize; i++)
        {
            SpawnObject();
            totalSpawned++;
        }
    }
    void OnDrawGizmos()
    {

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position, spawnAreaSize);
    }
}
