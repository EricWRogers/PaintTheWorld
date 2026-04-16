using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Menus : MonoBehaviour
{
    [Header("Scene")]
    public string sceneToLoad;

    [Header("Pause Menu Panels")]
    public GameObject mainButtonsPanel;
    public GameObject settingsPanel;

    void Start()
    {
        GameManager.instance.PauseGame();

        if (mainButtonsPanel != null)
            mainButtonsPanel.SetActive(true);

        if (settingsPanel != null)
            settingsPanel.SetActive(false);
    }

    public void PlayGame()
    {
        GameManager.instance.ResumeGame();
        SceneManager.LoadScene(sceneToLoad);
    }

    public void OpenSettings()
    {
        if (mainButtonsPanel != null)
            mainButtonsPanel.SetActive(false);

        if (settingsPanel != null)
            settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        if (mainButtonsPanel != null)
            mainButtonsPanel.SetActive(true);
    }

    public void Resume(GameObject _pauseHud)
    {
        GameManager.instance.ResumeGame();
    }
    
    public void MainMenu(GameObject _pauseHud)
    {
        SceneManager.LoadScene("Start Menu");
        GameManager.instance.ResumeGame();
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