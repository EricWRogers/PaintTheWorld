using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class EnemyManager : SceneAwareSingleton<EnemyManager>
{
    public List<ObjectSpawner> spawners = new();
    public int maxNumberOfEnemies = 10;
    public int spawnDelay = 5;
    public int currentEnemies = 0;

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        spawners.Clear();
        spawners.AddRange(FindObjectsByType<EnemySpawning>(FindObjectsSortMode.None));
        IsReady = true;
    }

    public void SpawnEnemies(int amount)
    {
        if (spawners.Count == 0) return;

        for (int i = 0; i < amount; i++)
        {
            int index = Random.Range(0, spawners.Count);
            spawners[index].SpawnObject();
            currentEnemies++;
            if (currentEnemies >= maxNumberOfEnemies) break;
        }
    }

    public void RemoveEnemy()
    {
        currentEnemies = Mathf.Max(0, currentEnemies - 1);
    }
    public void EditorInit()
    {
        spawners.Clear();
        spawners.AddRange(GameObject.FindObjectsOfType<ObjectSpawner>());
        IsReady = true;
    }
}
