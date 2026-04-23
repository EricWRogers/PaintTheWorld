using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class EnemyManager : SceneAwareSingleton<EnemyManager>
{
    public List<EnemySpawning> m_groundSpawners = new();
    public List<EnemySpawning> m_flyingSpawners = new();
    public float spawnDelay = 2f;
    public int selectedArea;
    // public AnimationCurve enemyHealthScaling;
    public AnimationCurve enemyAmountScaling;
    public int flyingStartingAmount;
    public int groundStartingAmount;
    private int m_groundSpawnCounter;
    private int m_flyingSpawnCounter;
    private float m_timer;
    public int flyingTargetingPlayer;
    public int maxFlyingTargetingPlayer;
    public CloudNav cloudNav;

    void Start()
    {
        
    }
    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_groundSpawners.Clear();
        m_flyingSpawners.Clear();
        foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
        {
            if (spawner.flyingSpawner)
            {
                m_flyingSpawners.Add(spawner);
            }
            else
            {
                m_groundSpawners.Add(spawner);
            }
        }
        m_groundSpawnCounter = 0;
        m_flyingSpawnCounter = 0;
        cloudNav = FindAnyObjectByType<CloudNav>(FindObjectsInactive.Exclude);

        IsReady = true;
    }

    void Update()
    {
        if(m_flyingSpawners.Count == 0 )
        {
            foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
            {
                if (spawner.flyingSpawner)
                {
                    m_flyingSpawners.Add(spawner);
                }
            }
            return;
        }
        if(m_groundSpawners.Count == 0 )
        {
            foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
            {
                if (!spawner.flyingSpawner)
                {
                    m_groundSpawners.Add(spawner);
                    
                }
            }
            return;
        }
        if(cloudNav == null)
        {
            cloudNav = FindAnyObjectByType<CloudNav>(FindObjectsInactive.Exclude);
            return;
        }
        m_timer -= Time.deltaTime;
        if(m_timer <= 0)
        {
            if ((int)(enemyAmountScaling.Evaluate(GameManager.instance.stageCounter - 1) + groundStartingAmount) > m_groundSpawnCounter && m_groundSpawners.Count != 0 )
            {
                ChooseSpawnArea(false);
                
                Enemy ground = m_groundSpawners[selectedArea].SpawnEnemy();
                m_groundSpawnCounter++;
            }
            if ((int)(enemyAmountScaling.Evaluate(GameManager.instance.stageCounter - 1) + flyingStartingAmount) > m_flyingSpawnCounter && m_flyingSpawners.Count != 0 )
            {
                ChooseSpawnArea(true);
                
                Enemy flying = m_flyingSpawners[selectedArea].SpawnEnemy();
                flying.targetingPlayer = true;
                m_flyingSpawnCounter++;
                
            }
            m_timer = spawnDelay;
        }
    }

    public void ChooseSpawnArea(bool _flying)
    {
        if (_flying)
        {
            if(m_flyingSpawners.Count == 0)
            {
                return;
            }
            selectedArea = Random.Range(0, m_flyingSpawners.Count);
        }
        else
        {
            if(m_groundSpawners.Count == 0)
            {
                return;
            }
            selectedArea = Random.Range(0, m_groundSpawners.Count);
        }

        
    }
    public void EditorInit()
    {
        m_groundSpawners.Clear();
        m_flyingSpawners.Clear();
        foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
        {
            if (spawner.flyingSpawner)
            {
                m_flyingSpawners.Add(spawner);
            }
            else
            {
                m_groundSpawners.Add(spawner);
            }

        IsReady = true;
        }
    }

    public PaintingObj GetObjectiveTarget()
    {
        List<PaintingObj> objList = GameManager.instance.activeObjectives;
        PaintingObj currentLowest = objList[0];
        if(currentLowest.currentEnemiesTarget == 0)
        {
            currentLowest.currentEnemiesTarget++;
            return currentLowest;
        }
        for(int i = 1; i < objList.Count; i++)
        {
            if(objList[i].currentEnemiesTarget < currentLowest.currentEnemiesTarget)
            {
                currentLowest = objList[i];
            }
            if(currentLowest.currentEnemiesTarget == 0)
            {
                currentLowest.currentEnemiesTarget++;
                return currentLowest;
            }
        }
        currentLowest.currentEnemiesTarget++;
        return currentLowest;
    }
}
