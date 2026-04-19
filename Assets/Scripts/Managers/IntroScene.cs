using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroScene : MonoBehaviour
{
    public Animator Anim;
    AsyncOperation async;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        async = SceneManager.LoadSceneAsync("SubwayScene");
        async.allowSceneActivation = false;
    }
    public void AnimDone()
    {
        async.allowSceneActivation = true;
    }
}
