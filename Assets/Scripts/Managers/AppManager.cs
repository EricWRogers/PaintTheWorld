using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AppManager : Singleton<AppManager>
{
    public string shopScene;
    public int currentSceneIndex;

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
    }

    void Start()
    {
        
        for(int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).gameObject.name == "AudioManagerObject" || transform.GetChild(i).gameObject.name == "ObjectPools")
                {
                    continue;
                }
                transform.GetChild(i).gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            AudioManager.instance.PlayMusic(AudioManager.instance.StartMusic);
    }

    private void OnActiveSceneChanged(Scene _current, Scene _next)
    {
        if(_next.buildIndex == 0)
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                if(transform.GetChild(i).gameObject.name == "AudioManagerObject" || transform.GetChild(i).gameObject.name == "ObjectPools")
                {
                    continue;
                }
                transform.GetChild(i).gameObject.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            AudioManager.instance.PlayMusic(AudioManager.instance.StartMusic);
        }
        else
        {
            for(int i = 0; i < transform.childCount; i++)
            {
                transform.GetChild(i).gameObject.SetActive(true);
            }
            if(_next.name != shopScene)
            {
                GameManager.instance.ResetManager();
            }
            //AudioManager.instance.FadeToMusic(AudioManager.instance.levelMusic);
        }
        
    }

    public void ChangeScene(string _sceneName, float _minTime)
    {
        Debug.Log("Start Loading Scene");
        AsyncOperation async = SceneManager.LoadSceneAsync(_sceneName);
        while(_minTime >= 0)
        {
            Debug.Log("Time Left: " + _minTime);
            _minTime -= Time.deltaTime;
            async.allowSceneActivation = false;
        }
        Debug.Log("Timer Done Activate Scene");
        async.allowSceneActivation = true;

    }
}
