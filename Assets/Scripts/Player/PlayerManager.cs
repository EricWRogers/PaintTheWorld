using Unity.VisualScripting;
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

public class PlayerManager : Singleton<PlayerManager>
{
    public GameObject player;
    public Health health;
    public Currency wallet;
    public Inventory inventory;
    public PlayerStats stats;
    public List<ItemStack> playerItems = new();
    public List<SkillData> playerSkills;

    private SaveData saveData; //= new SaveData();
    private const string SAVE_KEY = "PlayerSaveData";
    private SaveData startSaveData;
    private bool m_cursorEnabled;
    new void Awake()
    {
        base.Awake();
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return;
        }
        InitializeComponents();
        Debug.Log("initialized components");
        SaveGame();
        LoadGame();
    }
    void Start()
    {

        startSaveData = new SaveData
        (
            wallet.amount,
            health.currentHealth,
            health.maxHealth,
            playerItems,
            playerSkills
        );
    }
    private void InitializeComponents()
    {
        // Get required components
        wallet = player.GetComponent<Currency>() == null ? player.AddComponent<Currency>() : player.GetComponent<Currency>();
        health = player.GetComponent<Health>() == null ? player.AddComponent<Health>() : player.GetComponent<Health>();
        inventory = player.GetComponent<Inventory>() == null ? player.AddComponent<Inventory>() : player.GetComponent<Inventory>();
        stats = player.GetComponent<PlayerStats>() == null ? player.AddComponent<PlayerStats>() : player.GetComponent<PlayerStats>();

    }

    public void SaveGame()
    {
        saveData = new SaveData
        (
            wallet.amount,
            health.currentHealth,
            health.maxHealth,
            playerItems,
            stats.skills
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
        wallet.amount = saveData.coins;
        health.currentHealth = saveData.health;
        health.maxHealth = saveData.maxHealth;
        inventory.items = saveData.playerItems;
        stats.skills = saveData.playerSkills;
        Debug.Log("Game loaded successfully");

    }
    public void RegisterPlayer(GameObject _player)
    {
        player = _player;
        print("player registered");
    }


    // Auto-save when application quits
    private void OnApplicationQuit()
    {
        SaveGame();
    }
    public void OnDeath()
    {
        Debug.Log("Player Died");
        SceneManager.LoadSceneAsync(0);
        ResetData();

    }

    public void ResetData()
    {
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(startSaveData, true));
        Debug.Log("Save data reset.");
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
