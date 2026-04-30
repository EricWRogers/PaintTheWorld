using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    public Animator Anim;
    AsyncOperation async;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        async = SceneManager.LoadSceneAsync("TutorialLevel");
        async.allowSceneActivation = false;
    }
    public void AnimDone()
    {
        // Fade to level music before activating the scene
        if (AudioManager.instance != null)
        {
           AudioManager.instance.PlayMusic(AudioManager.instance.levelMusic);
        }
        async.allowSceneActivation = true;
    }
}
