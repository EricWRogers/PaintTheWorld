using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using SuperPupSystems.Helper;
using System.Collections.Generic;

public class ScenePortal : MonoBehaviour
{
    public string playerTag = "Player";
    public KeyCode interactKey = KeyCode.E;

    [Header("Optional UI")]
    public GameObject promptUI;
    public TMP_Text promptText;

    [Header("Stage Selection")]
    public List<string> stageScenes; 
    public bool useGameManagerStages = true;

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
            string chosenScene = GetRandomStageScene();

            if (string.IsNullOrEmpty(chosenScene))
            {
                Debug.LogWarning("ScenePortal: No valid stage scene found.");
                return;
            }

            GameManager.instance.GetComponent<Timer>().StartTimer();
            SceneManager.LoadScene(chosenScene);
        }
    }

    string GetRandomStageScene()
    {
        List<string> scenesToUse = null;

        if (useGameManagerStages && GameManager.instance != null && GameManager.instance.stageScenes != null && GameManager.instance.stageScenes.Count > 0)
        {
            scenesToUse = GameManager.instance.stageScenes;
        }
        else if (stageScenes != null && stageScenes.Count > 0)
        {
            scenesToUse = stageScenes;
        }

        if (scenesToUse == null || scenesToUse.Count == 0)
            return null;

        int randomIndex = Random.Range(0, scenesToUse.Count);
        return scenesToUse[randomIndex];
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