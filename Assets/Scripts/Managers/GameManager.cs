
using System;
using System.Collections.Generic;
using KinematicCharacterControler;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR

using UnityEditor;
[CustomEditor(typeof(GameManager))]

public class GameManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        GameManager gm = (GameManager)target;

        if (GUILayout.Button("Save Game"))
        {
            gm.SaveGame();
        }
        if (GUILayout.Button("Load Game"))
        {
            gm.LoadGame();
        }
        if (GUILayout.Button("Reset Save Data"))
        {
            gm.ResetData();
        }
    }
    
}
#endif

[RequireComponent(typeof(Timer))]
public class GameManager : SceneAwareSingleton<GameManager>
{
    private PlayerManager pm;

    public GameObject pauseMenu;

    private bool gameplayStarted = false;

    public bool m_isPaused;
    public String  shopScene;
    public int stageCounter = 1;

    private SaveData saveData;
    private const string SAVE_KEY = "GameSaveData";
    private SaveData startSaveData;

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(pm = null)
        {
            pm = PlayerManager.instance;
        }
        if (startSaveData == null)
            //startSaveData = new SaveData(0, pm.startingHealth, pm.startingHealth, new(), new(), 1);
        gameplayStarted = false;
        IsReady = true;
        
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
        ResetData();
    }
    public void EditorInit()
    {
        IsReady = true;
    }

    public void ShopStage()
    {
        SceneManager.LoadScene(shopScene);

    }

    public void SaveGame()
    {
        saveData = new SaveData(
            pm.wallet.amount,
            pm.health.currentHealth,
            pm.health.maxHealth,
            MakeInvEntries(pm.inventory),
            pm.stats.skills,
            stageCounter
        );

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully");
    }

    public void LoadGame()
    {
        string json = PlayerPrefs.GetString(SAVE_KEY);
        Debug.Log("Loading JSON: " + json);
        saveData = JsonUtility.FromJson<SaveData>(json);
        // Apply loaded data
        Debug.Log("Loaded JSON: " + json);
        pm.health.currentHealth = saveData.health;
        pm.health.maxHealth = saveData.maxHealth;
        pm.wallet.amount = saveData.coins;
        stageCounter = saveData.stageCounter;
        SceneManager.LoadScene(shopScene);
        
    }
    // public void OnDeath()
    // {
    //     ResetData();
    //     GameManager.instance.ResetGame();
    //     SceneManager.sceneLoaded += OnSceneReloaded;
    //     SceneManager.LoadSceneAsync("MainMenu");
    // }

    private void OnSceneReloaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneReloaded;
        LoadGame();
    }

    public void ResetData()
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(startSaveData, true));
        PlayerPrefs.Save();
        Debug.Log("Save data reset.");
        LoadGame();
    }

    private static List<InvEntry> MakeInvEntries(Inventory inv)
    {
        var list = new List<InvEntry>();
        if (inv != null && inv.items != null)
        {
            foreach (var s in inv.items)
                if (s != null && s.item != null)
                    list.Add(new InvEntry { id = s.item.id, count = s.count });
        }
        return list;
    }
}

[System.Serializable]
public class SaveData
{
    public int health;
    public int coins;
    public int maxHealth = 100;
    public List<InvEntry> items = new();
    public List<SkillData> playerSkills;
    public int stageCounter;

    public SaveData(int _coins, int _health, int _maxHealth,
                    List<InvEntry> _items, List<SkillData> _skills, int _stageCounter)
    {
        coins = _coins;
        health = _health;
        maxHealth = _maxHealth;
        items = _items;
        playerSkills = _skills;
        stageCounter = _stageCounter;
    }

    
}
