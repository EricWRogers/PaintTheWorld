using UnityEngine;
using UnityEngine.SceneManagement;

public static class ManagerBootstrap
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Initialize()
    {
        // Load prefabs from Resources
        LoadManagerPrefab<GameManager>("Managers/GameManager");
        LoadManagerPrefab<PlayerManager>("Managers/PlayerManager");
        //LoadManagerPrefab<EnemyManager>("Managers/EnemyManager");

        Debug.Log("ManagerBootstrap: Prefabs loaded before scene start.");
    }

    private static void LoadManagerPrefab<T>(string path) where T : MonoBehaviour
    {
        // Only load if it doesn't already exist
        if (Object.FindObjectOfType<T>() != null)
            return;

        T prefab = Resources.Load<T>(path);
        if (prefab == null)
        {
            Debug.LogWarning($"ManagerBootstrap: Could not find prefab at {path}");
            return;
        }

        T instance = Object.Instantiate(prefab);
        instance.name = typeof(T).Name;
        Object.DontDestroyOnLoad(instance.gameObject);
    }
}