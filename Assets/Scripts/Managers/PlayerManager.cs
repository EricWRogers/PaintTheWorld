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

[System.Serializable] public class InvEntry { public string id; public int count; }
public class PlayerManager : SceneAwareSingleton<PlayerManager>
{
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
    public GameObject player;
    public Health health;
    public Currency wallet;
    public Inventory inventory;
    public PlayerStats stats;

    private float m_healthMult => PlayerManager.instance.stats.skills[0].currentMult;
    public int startingHealth;

    private SaveData saveData;
    private const string SAVE_KEY = "PlayerSaveData";
    private SaveData startSaveData;

    [Header("Item Runtime Context")]
    public LayerMask enemyLayer;
    public GameObject paintGlobPrefab;
    public GameObject healAuraPrefab;
    public float globSpeed = 18f;

    public PlayerContext GetContext() => new PlayerContext {
        player = player ? player.transform : null,
        playerHealth = health,
        enemyLayer = enemyLayer,
        paintGlobPrefab = paintGlobPrefab,
        healAuraPrefab = healAuraPrefab,
        globSpeed = globSpeed
    };

    void Start()
    {
        startingHealth = health.currentHealth;
    }


    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            RegisterPlayer(player);
            if (startSaveData == null)
                startSaveData = new SaveData(wallet.amount, health.currentHealth, health.maxHealth, MakeInvEntries(inventory), stats.skills);
            IsReady = true;
            health.maxHealth = Mathf.RoundToInt(startingHealth * m_healthMult);
            health.currentHealth = health.maxHealth;
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
        wallet = GetComponent<Currency>() ?? gameObject.AddComponent<Currency>();
        health = GetComponent<Health>() ?? gameObject.AddComponent<Health>();
        inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();
        stats = GetComponent<PlayerStats>() ?? gameObject.AddComponent<PlayerStats>();
    }

    public void SaveGame()
    {
        saveData = new SaveData(
            wallet.amount,
            health.currentHealth,
            health.maxHealth,
            MakeInvEntries(inventory),
            stats.skills
        );

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully");
    }

    public void LoadGame()
    {
        saveData = new SaveData(
            wallet.amount,
            health.currentHealth,
            health.maxHealth,
            MakeInvEntries(inventory),
            stats.skills
        );

        string json = JsonUtility.ToJson(saveData, true);
        PlayerPrefs.SetString(SAVE_KEY, json);
        PlayerPrefs.Save();
        Debug.Log("Game saved successfully");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }
    public void OnDeath()
    {
        ResetData();
        SceneManager.sceneLoaded += OnSceneReloaded;
        SceneManager.LoadSceneAsync("MainMenu");
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
        Debug.Log("editorInit");
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
    public int maxHealth = 100;
    public List<InvEntry> items = new();
    public List<SkillData> playerSkills;

    public SaveData(int _coins, int _health, int _maxHealth,
                    List<InvEntry> _items, List<SkillData> _skills)
    {
        coins = _coins;
        health = _health;
        maxHealth = _maxHealth;
        items = _items;
        playerSkills = _skills;
    }
}
