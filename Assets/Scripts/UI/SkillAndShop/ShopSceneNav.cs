using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopSceneNav : MonoBehaviour
{
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] string nextScene = "GameStage01";
    [SerializeField] string testScene = "ItemsTesting";

    public void GoMainMenu() => SceneManager.LoadScene(mainMenuScene);
    public void GoNext() => SceneManager.LoadScene(nextScene);


    public void GoTestScene() => SceneManager.LoadScene(testScene);
    
    public void HandleCursor()
        {
        if (Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        }
}

