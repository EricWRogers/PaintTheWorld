using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using System.Collections;

#if UNITY_EDITOR
using UnityEditor;

[InitializeOnLoad]
public static class EditorBootstrap
{
    static EditorBootstrap()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.EnteredPlayMode)
        {
            // Force-initialize all managers
            PlayerManager.instance.EditorInit();
            //EnemyManager.instance.EditorInit();
            GameManager.instance.EditorInit();

            // Begin gameplay immediately if needed
            GameManager.instance.BeginGameplay();
        }
    }
}
#endif

public static class ManagerLoader
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitializeManagers()
    {
        _ = PlayerManager.instance;
        //_ = EnemyManager.instance;
        _ = GameManager.instance;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var managers = GameObject.FindObjectsOfType<MonoBehaviour>()
            .OfType<ISceneLoadHandler>()
            .ToArray();

        foreach (var m in managers)
            m.OnSceneLoaded(scene, mode);

        GameManager.instance.StartCoroutine(WaitUntilAllReady(managers));
    }

    private static IEnumerator WaitUntilAllReady(ISceneLoadHandler[] managers)
    {
        while (!managers.All(m => m.IsReady))
            yield return null;

        GameManager.instance.BeginGameplay();
    }
}

