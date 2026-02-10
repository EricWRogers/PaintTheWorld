using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SuperPupSystems.Helper;




[System.Serializable] public class InvEntry { public string id; public int count; }
public class PlayerManager : SceneAwareSingleton<PlayerManager>
{
    
    public GameObject player;
    public Health health;
    public Currency wallet;
    public Inventory inventory;
    public PlayerStats stats;

    //private float m_healthMult => stats.skills[0].currentMult;
    public int startingHealth;

    public PlayerInputActions.PlayerActions playerInputs;
    public PlayerInputActions.UIActions uIInputs;

    [Header("Item Runtime Context")]
    public LayerMask enemyLayer;
    public GameObject paintGlobPrefab;
    public GameObject paintBurstPrefab;
    public GameObject healAuraPrefab;
    public float globSpeed = 18f;
    public int maxJumpCount = 1;

    public PlayerContext GetContext() => new PlayerContext
    {
        player = player ? player.transform : null,
        playerHealth = health,
        enemyLayer = enemyLayer,
        paintGlobPrefab = paintGlobPrefab,
        paintBurstPrefab = paintBurstPrefab,
        healAuraPrefab = healAuraPrefab,
        globSpeed = globSpeed
    };

    new void Awake()
    {
        base.Awake();
        playerInputs = new PlayerInputActions().Player;
        uIInputs = new PlayerInputActions().UI;
        uIInputs.Disable();
        playerInputs.Enable();
    }

    void Start()
    {
    }


    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            RegisterPlayer(player);
            IsReady = true;
            health.maxHealth = Mathf.RoundToInt(startingHealth);
            health.currentHealth = health.maxHealth;

        }
    }

    void Update()
    {
        health.maxHealth = Mathf.RoundToInt(startingHealth);
        if (playerInputs.Pause.IsPressed())
        {
            GameManager.instance.PauseGame();
            GameManager.instance.pauseMenu.SetActive(true);
        }
        if (uIInputs.Resume.IsPressed())
        {
            GameManager.instance.ResumeGame();
            GameManager.instance.pauseMenu.SetActive(false);
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

    
    public void EditorInit()
    {
        Debug.Log("editorInit");
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            RegisterPlayer(player);

        IsReady = true;
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        playerInputs.Disable();
    }
}


