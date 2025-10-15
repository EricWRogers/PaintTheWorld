using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus : MonoBehaviour
{
    void Start()
    {
        GameManager.instance.PauseGame();
    }
    public void PlayGame()
    {
        GameManager.instance.ResumeGame();
        SceneManager.LoadScene(0);
    }
    public void Options()
    {

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
