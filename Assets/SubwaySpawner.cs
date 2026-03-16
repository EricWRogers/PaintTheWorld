using UnityEngine;

public class SubwaySpawner : MonoBehaviour
{
    public GameObject prefabToSpawn;
    public void Spawn() {
        Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
    }
}
