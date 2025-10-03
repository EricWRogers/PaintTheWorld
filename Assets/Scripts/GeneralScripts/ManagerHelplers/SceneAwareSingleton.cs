using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class SceneAwareSingleton<T> : Singleton<T>, ISceneLoadHandler where T : MonoBehaviour
{
    public virtual bool IsReady { get; protected set; } = false;
    public override void Awake()
    {
        base.Awake();
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    protected virtual void OnDestroy()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        OnSceneLoaded(scene, mode);
    }
    public virtual void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }
}