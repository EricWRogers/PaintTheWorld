using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SuperPupSystems.Helper;

[RequireComponent(typeof(Timer))]
public class EnemyManager : SceneAwareSingleton<EnemyManager>
{
    public List<ObjectSpawner> spawners = new();
    public int baseMaxEnemyCount = 15;
    public int spawnDelay = 5;
    public int currentEnemies = 0;
    public int scaledCount;
    public int spawnAmount = 1;
    private Timer m_spawnTimer;

    void Start()
    {
        m_spawnTimer = GetComponent<Timer>();
    }
    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_spawnTimer.StartTimer(spawnDelay, true);
        m_spawnTimer.timeout.AddListener(Spawn);
        spawners.Clear();
        spawners.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));
        IsReady = true;
    }

    private void Spawn()
    {
        SpawnEnemies(spawnAmount);
    }

    public void SpawnEnemies(int amount)
    {
        if (spawners.Count == 0) return;

        scaledCount = Mathf.RoundToInt(baseMaxEnemyCount * GameManager.instance.EnemyCountModifier);


        for (int i = 0; i < amount; i++)
        {
            if (currentEnemies <= scaledCount)
            {
                int index = Random.Range(0, spawners.Count);
                spawners[index].SpawnObject();
                currentEnemies++;
            }
        }
    }

    public void RemoveEnemy()
    {
        currentEnemies = Mathf.Max(0, currentEnemies - 1);
    }
    public void EditorInit()
    {
        m_spawnTimer.StartTimer(spawnDelay, true);
        m_spawnTimer.timeout.AddListener(Spawn);   
        spawners.Clear();
        spawners.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));
        IsReady = true;
    }
}
