using UnityEngine.SceneManagement;

public interface ISceneLoadHandler
{
    bool IsReady { get; }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode);
}