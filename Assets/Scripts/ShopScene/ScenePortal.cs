using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class ScenePortal : MonoBehaviour
{
    public string playerTag = "Player";
    public string sceneToLoad = "YourSceneNameHere";
    public KeyCode interactKey = KeyCode.E;

    [Header("Optional UI")]
    public GameObject promptUI;       
    public TMP_Text promptText;

    private bool canUse = false;

    void Start()
    {
        if (promptUI) promptUI.SetActive(false);
    }

    void Update()
    {
        if (!canUse) return;

        if (Input.GetKeyDown(interactKey))
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        canUse = true;

        if (promptUI) promptUI.SetActive(true);
        if (promptText) promptText.text = $"Press {interactKey} to exit the shop";
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;
        canUse = false;

        if (promptUI) promptUI.SetActive(false);
    }
}
