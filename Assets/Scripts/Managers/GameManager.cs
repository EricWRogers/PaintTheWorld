using System.Collections.Generic;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;
using HutongGames.PlayMaker.Actions;




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
    public float timePerStage;
    public List<string> stageScenes;
    public bool m_isPaused;
    public string shopScene;
    public string mainMenu;
    public string nextScene;
    public string tutorialScene;
    public int stageCounter = 1;
    public List<PaintingObj> objectives;
    public List<PaintingObj> activeObjectives;
    public int numberOfActiveHoldObjectives = 2;
    public float percentTimeHeldToClear;
    private float m_heldGoal;
    public float timeHeld;
    public int numberOfActiveCaptureObjectives = 1;
    public int captureAmountToClear = 5;
    public int amountCaptured;
    private bool playerSpawned;
    public enum gameModes {HoldPoints, CapturePoints}

    public gameModes currentGamemode;

    private SaveData saveData;
    private const string SAVE_KEY = "GameSaveData";
    private SaveData startSaveData;
    public bool inStage;
    public bool sceneHasPlayer;
    public GameObject ui;

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        inStage = false;
        for(int i = 0; i < stageScenes.Count; i++)
        {
            if(SceneManager.GetActiveScene().name == stageScenes[i])
            {
                inStage = true;
            }
        }
        if(SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().buildIndex == 1 || SceneManager.GetActiveScene().buildIndex == 3)
        {
            ui.SetActive(false);
            sceneHasPlayer = false;
        }
        else
        {
            ui.SetActive(true);
            sceneHasPlayer = true;
        }
        if(pm = null)
        {
            pm = PlayerManager.instance;
        }
        objectives.Clear();
        activeObjectives.Clear();
        objectives.AddRange(FindObjectsByType<PaintingObj>(FindObjectsSortMode.None));
        foreach(PaintingObj paint in objectives)
        {
            paint.transform.parent.gameObject.SetActive(false);
        }
        if (inStage)
        {
            ResetManager();
            m_heldGoal = timePerStage / percentTimeHeldToClear / 100;
        }
        PlayerManager.instance.playerInputs.Enable();
        PlayerManager.instance.uIInputs.Disable();
        if (startSaveData == null)
            //startSaveData = new SaveData(0, pm.startingHealth, pm.startingHealth, new(), new(), 1);
        IsReady = true;
        
    }

    void Update()
    {

        // if (Input.GetKeyDown(KeyCode.U)){
        //     SceneManager.LoadSceneAsync(shopScene);
        // }
        if (!m_isPaused && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        if (!inStage)
        {
            GetComponent<Timer>().StopTimer();
            return;
        }
        SpawnObjectives();
    }

    public void RemoveObjective(PaintingObj _objectiveToRemove)
    {
        activeObjectives.Remove(_objectiveToRemove);
        _objectiveToRemove.transform.parent.gameObject.SetActive(false);
    }

    public void SpawnObjectives()
    {
        if(currentGamemode == gameModes.HoldPoints)
        {
            if(activeObjectives.Count < numberOfActiveHoldObjectives)
            {
                int randInt = Random.Range(0, objectives.Count);
                if(objectives[randInt].transform.parent.gameObject.activeInHierarchy)
                    return;
                objectives[randInt].transform.parent.gameObject.SetActive(true);
                activeObjectives.Add(objectives[randInt]);
            }
            else if(!playerSpawned)
            {
                int randSpawn = Random.Range(0, activeObjectives.Count);
                PlayerManager.instance.player.transform.position = activeObjectives[randSpawn].playerSpawnPoint.position;
                PlayerManager.instance.player.transform.rotation = activeObjectives[randSpawn].playerSpawnPoint.rotation;
                playerSpawned = true;

            }
        }
        else if(currentGamemode == gameModes.CapturePoints)
        {
            if(activeObjectives.Count < numberOfActiveCaptureObjectives)
            {
                int randInt = Random.Range(0, objectives.Count);
                if(objectives[randInt].transform.parent.gameObject.activeInHierarchy)
                    return;
                objectives[randInt].transform.parent.gameObject.SetActive(true);
                activeObjectives.Add(objectives[randInt]);
            }
            else if(!playerSpawned)
            {
                int randSpawn = Random.Range(0, activeObjectives.Count);
                PlayerManager.instance.player.transform.position = activeObjectives[randSpawn].playerSpawnPoint.position;
                PlayerManager.instance.player.transform.rotation = activeObjectives[randSpawn].playerSpawnPoint.rotation;
                playerSpawned = true;

            }
        }

        
    }
    public void PauseGame()
    {
        m_isPaused = true;
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlayerManager.instance.playerInputs.Disable();
        PlayerManager.instance.uIInputs.Enable();
    }
    
    public void ResumeGame()
    {
        m_isPaused = false;
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
    public void StageOver()
    {
        if(currentGamemode == gameModes.HoldPoints)
        {
            if(m_heldGoal >= timeHeld)
            {
                nextScene = shopScene;
            }
            else
            {
                nextScene = mainMenu;
            }
        }
        else if(currentGamemode == gameModes.CapturePoints)
        {
            if(amountCaptured >= captureAmountToClear)
            {
                nextScene = shopScene;
            }
            else
            {
                nextScene = mainMenu;
            }
        }
    }

    public void ShopStage()
    {
        SceneManager.LoadSceneAsync("LoadScene");

    }
    public void ResetManager()
    {
        currentGamemode = (gameModes)Random.Range(0, System.Enum.GetValues(typeof(gameModes)).Length);
        GetComponent<Timer>().StartTimer(timePerStage);
        playerSpawned = false;
        timeHeld = 0;
        amountCaptured = 0;
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
        SceneManager.LoadSceneAsync(shopScene);
        
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
