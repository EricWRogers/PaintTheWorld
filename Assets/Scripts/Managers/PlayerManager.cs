using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SuperPupSystems.Helper;


#if UNITY_EDITOR

using UnityEditor;
[CustomEditor(typeof(PlayerManager))]

public class PlayerManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        PlayerManager pm = (PlayerManager)target;

        if (GUILayout.Button("Save Game"))
        {
            pm.SaveGame();
        }
        if (GUILayout.Button("Load Game"))
        {
            pm.LoadGame();
        }
        if (GUILayout.Button("Reset Save Data"))
        {
            pm.ResetData();
        }
    }
    
}
#endif

public class PlayerManager : SceneAwareSingleton<PlayerManager>
{
    public GameObject player;
    public Health health;
    public Currency wallet;
    public Inventory inventory;
    public PlayerStats stats;

    private SaveData saveData;
    private const string SAVE_KEY = "PlayerSaveData";
    private SaveData startSaveData;
    new void Awake()
    {
        base.Awake();
    }


    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            RegisterPlayer(player);
            startSaveData = new SaveData(wallet.amount, health.currentHealth, health.maxHealth, inventory.items, stats.skills);
            IsReady = true;
        }
    }

    public void RegisterPlayer(GameObject newPlayer)
    {
        player = newPlayer;
        InitializeComponents();

        Debug.Log("Player registered in PlayerManager");
    }

    private void InitializeComponents()
    {
        wallet = player.GetComponent<Currency>() ?? player.AddComponent<Currency>();
        health = player.GetComponent<Health>() ?? player.AddComponent<Health>();
        inventory = player.GetComponent<Inventory>() ?? player.AddComponent<Inventory>();
        stats = player.GetComponent<PlayerStats>() ?? player.AddComponent<PlayerStats>();
    }

    public void SaveGame()
    {
        saveData = new SaveData
        (
            wallet.amount,
            health.currentHealth,
            health.maxHealth,
            inventory.items,
            stats.skills
        );
        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully");
    }

    public void LoadGame()
    {
        if (!PlayerPrefs.HasKey(SAVE_KEY)) return;
        string json = PlayerPrefs.GetString(SAVE_KEY);
        Debug.Log("Loading JSON: " + json);
        saveData = JsonUtility.FromJson<SaveData>(json);
        if (saveData == null) return;
        // Apply loaded data
        Debug.Log("Loaded JSON: " + json);
        wallet.amount = saveData.coins;
        health.currentHealth = saveData.health;
        health.maxHealth = saveData.maxHealth;
        inventory.items.Clear();
        inventory.items.AddRange(saveData.playerItems);
        stats.skills.Clear();
        stats.skills.AddRange(saveData.playerSkills);
        Debug.Log("Game loaded successfully");

    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
    public void OnDeath()
    {
        SceneManager.sceneLoaded += OnSceneReloaded;
        SceneManager.LoadSceneAsync(0);
    }

    private void OnSceneReloaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneReloaded;
        LoadGame();
    }

    public void ResetData()
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(startSaveData, true));
        Debug.Log("Save data reset.");
        LoadGame();
    }
    public void EditorInit()
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            RegisterPlayer(player);

        IsReady = true;
    }
}

[System.Serializable]
public class SaveData
{
    public int health;
    public int coins;
    public List<ItemStack> playerItems = new();
    public int maxHealth = 100;
    public List<SkillData> playerSkills;

    public SaveData(int _coins, int _health, int _maxHealth,
    List<ItemStack> _playerItems, List<SkillData> _playerSkills)
    {
        coins = _coins;
        health = _health;
        maxHealth = _maxHealth;
        playerItems = _playerItems;
        playerSkills = _playerSkills;


    }
    

    
}
