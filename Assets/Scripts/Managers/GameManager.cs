
using System.Collections.Generic;
using KinematicCharacterControler;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Timer))]
public class GameManager : SceneAwareSingleton<GameManager>
{
    public GameObject pauseMenu;
    public Timer timer;
    
    public List<PaintingObj> objectives;
    public bool allObjComplete;
    public int amountOfObjComplete;

    private bool gameplayStarted = false;

    public bool m_isPaused;

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        gameplayStarted = false;
        IsReady = true;
    }

    void Update()
    {
        if(objectives.Count == amountOfObjComplete)
        {
            allObjComplete = true;
        }
    }

    public void BeginGameplay()
    {
        if (gameplayStarted) return;
        gameplayStarted = true;
    }
    public void PauseGame()
    {
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerManager.instance.playerInputs.Disable();
        PlayerManager.instance.uIInputs.Enable();
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        PlayerManager.instance.playerInputs.Enable();
        PlayerManager.instance.uIInputs.Disable();
    }

    

    public void ResetGame()
    {
        PlayerManager.instance.ResetData();
    }
    public void EditorInit()
    {
        IsReady = true;
    }

    
}
