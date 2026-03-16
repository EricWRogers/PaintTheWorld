using UnityEngine;
using SuperPupSystems.Helper; 

public class SubwaySpawner : MonoBehaviour
{
    public Timer targetTimer;
    public GameObject prefabToSpawn;

    private bool _hasSpawnedThisCycle = false;
    private float _lastStartTime;

    void Start()
    {
        if (targetTimer == null)
            targetTimer = GetComponent<Timer>();
            
        if (targetTimer != null)
            _lastStartTime = targetTimer.countDownTime;
    }

    void Update()
    {
        if (targetTimer == null) return;

        float currentTime = targetTimer.timeLeft;

        if (currentTime > _lastStartTime * 0.9f) 
        {
            _hasSpawnedThisCycle = false;
        }

        if (!_hasSpawnedThisCycle && currentTime > 0 && currentTime <= (targetTimer.countDownTime * 0.5f))
        {
            Spawn();
            _hasSpawnedThisCycle = true;
        }
    }

    public void Spawn() 
    {
        if (prefabToSpawn != null)
        {
            Instantiate(prefabToSpawn, transform.position, Quaternion.identity);
        }
    }
}