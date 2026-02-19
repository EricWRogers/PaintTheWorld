using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class EnemyManager : SceneAwareSingleton<EnemyManager>
{
    public List<EnemySpawning> spawnerAreas = new();
    public float spawnDelay = .5f;
    public int selectedArea;
    public List<GameObject> listOfEnemyPrefabs;
    public AnimationCurve enemyHealthScaling;
    public AnimationCurve enemyAmountScaling;
    public int startingSpawnAmount;
    private int m_spawnCounter;
    private float m_timer;
    public int enemiesTargetingPlayer;
    public int enemiesProtectingObj;

    void Start()
    {
        
    }
    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        spawnerAreas.Clear();
        spawnerAreas.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));

        IsReady = true;
    }

    void Update()
    {
        if(spawnerAreas.Count == 0)
        {
            spawnerAreas.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));
            return;
        }
        m_timer -= Time.deltaTime;
        if ((int)(enemyAmountScaling.Evaluate(GameManager.instance.stageCounter) + startingSpawnAmount) > m_spawnCounter && m_timer <= 0)
        {
            ChooseSpawnArea();
            GameObject prefab = listOfEnemyPrefabs[Random.Range(0, listOfEnemyPrefabs.Count)];
            
            GameObject enemy = spawnerAreas[selectedArea].SpawnEnemy(prefab.name);
            enemy.GetComponent<Health>().maxHealth = (int)(enemy.GetComponent<Enemy>().startingHealth + enemyHealthScaling.Evaluate(GameManager.instance.stageCounter));
            if(m_spawnCounter % 2 == 0)
            {
                enemy.GetComponent<Enemy>().targetingPlayer = true;
            }
            else
            {
                enemy.GetComponent<Enemy>().targetingPlayer = false;
            }
            m_spawnCounter++;
            m_timer = spawnDelay;
        }

    }

    public void ChooseSpawnArea()
    {
        if(spawnerAreas.Count == 0)
        {
            return;
        }
        selectedArea = Random.Range(0, spawnerAreas.Count);
    }
    public void EditorInit()
    {
        spawnerAreas.Clear();
        spawnerAreas.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));

        IsReady = true;
    }
}
