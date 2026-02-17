using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus : MonoBehaviour
{
    public string sceneToLoad;
    void Start()
    {
        GameManager.instance.PauseGame();
    }
    public void PlayGame()
    {
        GameManager.instance.ResumeGame();
        SceneManager.LoadScene(sceneToLoad);
    }
    public void VolumeScroller(GameObject _pauseHud)
    {

    }

    public void Resume(GameObject _pauseHud)
    {
        GameManager.instance.ResumeGame();
        _pauseHud.SetActive(false);
    }
    
    public void MainMenu(GameObject _pauseHud)
    {
        SceneManager.LoadScene("Start Menu");
        GameManager.instance.ResumeGame();
        _pauseHud.SetActive(false);
    }

    public void Quit()
    {
        #if UNITY_EDITOR
        EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
        
    }
}
