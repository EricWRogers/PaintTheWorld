using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopSceneNav : MonoBehaviour
{
    [SerializeField] string mainMenuScene = "MainMenu";
    [SerializeField] string nextScene = "GameStage01";
    [SerializeField] string testScene = "ItemsTesting";  

    public void GoMainMenu() => SceneManager.LoadScene(mainMenuScene);
    public void GoNext()     => SceneManager.LoadScene(nextScene);

  
    public void GoTestScene() => SceneManager.LoadScene(testScene);
}

