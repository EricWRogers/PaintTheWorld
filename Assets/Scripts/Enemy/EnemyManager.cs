using System.Collections.Generic;
using SuperPupSystems.Helper;
using UnityEngine;

public class EnemyManager : Singleton<EnemyManager>
{
    public List<ObjectSpawner> spawners;
    public int maxNumberOfEnemies;
    public int spawnDelay;
    public int m_numberOfEnemies;
    private int m_ranSpawner;

    new void Awake()
    {
        base.Awake();
        Timer timer = GetComponent<Timer>();
        timer.autoStart = true;
        timer.countDownTime = spawnDelay;
        timer.autoRestart = true;
    }

    void Start()
    {
        AddEnemy(maxNumberOfEnemies / 4);
    }

    public void AddEnemy(int _amount)
    {
        m_ranSpawner = Random.Range(0, spawners.Count);

        for (int i = 0; i < _amount; i++)
        {
            if (m_numberOfEnemies < maxNumberOfEnemies)
            {
                spawners[m_ranSpawner].SpawnObject();
                m_numberOfEnemies++;
            }

        }
    }

    public void RemoveEnemy()
    {
        m_numberOfEnemies--;
    }
}
