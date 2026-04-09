using UnityEngine;

public class LoadingScreen : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        AppManager.instance.ChangeScene(GameManager.instance.stageScenes[Random.Range(0, GameManager.instance.stageScenes.Count)], 5f);
    }
}
