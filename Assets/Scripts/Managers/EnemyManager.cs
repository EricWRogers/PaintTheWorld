using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class EnemyManager : SceneAwareSingleton<EnemyManager>
{
    public List<EnemySpawning> spawnerAreas = new();
    public int spawnDelay = 1;
    public int scaledCount;
    public int spawnAmount = 1;
    public int selectedArea;
    public List<GameObject> listOfEnemyPrefabs;

    private int m_spawnCount;
    private float m_timer;

    void Start()
    {
        foreach (EnemySpawning go in spawnerAreas)
        {
            go.GetComponentInChildren<EnemySpawning>().indicator.SetActive(false);
        }
        ChooseSpawnArea();
    }
    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        spawnerAreas.Clear();
        spawnerAreas.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));

        IsReady = true;
    }

    void Update()
    {
        m_timer -= Time.deltaTime;
        if (spawnAmount > m_spawnCount && m_timer <= 0)
        {
            GameObject prefab = listOfEnemyPrefabs[Random.Range(0, listOfEnemyPrefabs.Count)];
            spawnerAreas[selectedArea].SpawnEnemy(prefab.name);
            m_spawnCount++;
            m_timer = spawnDelay;
        }

        foreach(EnemySpawning spawner in spawnerAreas)
        {
            if (spawner == spawnerAreas[selectedArea])
            {
                spawner.indicator.SetActive(true);
            }
            else
            {
                spawner.indicator.SetActive(false);
            }
        }

    }
    
    public void ResetWave()
    {
        spawnAmount = 0;
        m_spawnCount = 0;
    }

    public void ChooseSpawnArea()
    {
        if(spawnerAreas.Count == 0)
        {
            return;
        }
        spawnerAreas[selectedArea].indicator.SetActive(false);
        selectedArea = Random.Range(0, spawnerAreas.Count);
        spawnerAreas[selectedArea].indicator.SetActive(true);
    }

    public void EnemyKilled()
    {
        GameManager.instance.currKilledInWave++;
        GameManager.instance.totalEnemyKills++;
    }
    public void EditorInit()
    {
        spawnerAreas.Clear();
        spawnerAreas.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));

        IsReady = true;
    }
}
