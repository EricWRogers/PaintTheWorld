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
            SceneManager.LoadScene("LoadScene");
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