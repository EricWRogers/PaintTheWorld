using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using SuperPupSystems.Helper;

public class EnemyManager : SceneAwareSingleton<EnemyManager>
{
    public List<EnemySpawning> groundSpawners = new();
    public List<EnemySpawning> flyingSpawners = new();
    public List<Patroling> groundPatrols = new();
    public List<Patroling> airPatrols = new();
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
    private MapInfo m_mapInfo;

    void Start()
    {
        
    }
    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        groundSpawners.Clear();
        flyingSpawners.Clear();
        groundPatrols.Clear();
        airPatrols.Clear();
        m_mapInfo = FindAnyObjectByType<MapInfo>();
        GetMapInfo();
        foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
        {
            if (spawner.flyingSpawner)
            {
                flyingSpawners.Add(spawner);
            }
            else
            {
                groundSpawners.Add(spawner);
            }
        }
        foreach(Patroling patroling in FindObjectsByType<Patroling>(FindObjectsSortMode.None))
        {
            if (patroling.flyingPatrol)
            {
                airPatrols.Add(patroling);
            }
            else
            {
                groundPatrols.Add(patroling); 
            }
        }
        m_groundSpawnCounter = 0;
        m_flyingSpawnCounter = 0;
        cloudNav = FindAnyObjectByType<CloudNav>(FindObjectsInactive.Exclude);

        IsReady = true;
    }

    void Update()
    {
        if(flyingSpawners.Count == 0 )
        {
            foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
            {
                if (spawner.flyingSpawner)
                {
                    flyingSpawners.Add(spawner);
                }
            }
            return;
        }
        if(groundSpawners.Count == 0 )
        {
            foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
            {
                if (!spawner.flyingSpawner)
                {
                    groundSpawners.Add(spawner);
                    
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
            if ((int)(enemyAmountScaling.Evaluate(GameManager.instance.stageCounter - 1) + groundStartingAmount) > m_groundSpawnCounter && groundSpawners.Count != 0 )
            {
                ChooseSpawnArea(false);
                
                Enemy ground = groundSpawners[selectedArea].SpawnEnemy();
                
                ground.patroling = groundPatrols[m_groundSpawnCounter % groundPatrols.Count];
                m_groundSpawnCounter++;
            }
            if ((int)(enemyAmountScaling.Evaluate(GameManager.instance.stageCounter - 1) + flyingStartingAmount) > m_flyingSpawnCounter && flyingSpawners.Count != 0 )
            {
                ChooseSpawnArea(true);
                
                Enemy flying = flyingSpawners[selectedArea].SpawnEnemy();
                flying.patroling = airPatrols[m_flyingSpawnCounter % airPatrols.Count];
                m_flyingSpawnCounter++;
                
            }
            m_timer = spawnDelay;
        }
    }

    public void GetMapInfo()
    {
        if(m_mapInfo != null) return;

        flyingStartingAmount = m_mapInfo.flyingEnemyStartCount;
        groundStartingAmount = m_mapInfo.groundEnemyStartCount;
    }

    public void ChooseSpawnArea(bool _flying)
    {
        if (_flying)
        {
            if(flyingSpawners.Count == 0)
            {
                return;
            }
            selectedArea = Random.Range(0, flyingSpawners.Count);
        }
        else
        {
            if(groundSpawners.Count == 0)
            {
                return;
            }
            selectedArea = Random.Range(0, groundSpawners.Count);
        }

        
    }
    public void EditorInit()
    {
        groundSpawners.Clear();
        flyingSpawners.Clear();
        foreach(EnemySpawning spawner in FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None))
        {
            if (spawner.flyingSpawner)
            {
                flyingSpawners.Add(spawner);
            }
            else
            {
                groundSpawners.Add(spawner);
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
