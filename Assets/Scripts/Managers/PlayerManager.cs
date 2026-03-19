using UnityEngine;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using SuperPupSystems.Helper;

[System.Serializable]
public class InvEntry
{
    public string id;
    public int count;
}

public class PlayerManager : SceneAwareSingleton<PlayerManager>
{
    public GameObject player;
    public Health health;
    public Currency wallet;
    public Inventory inventory;
    public PlayerStats stats;

    public int startingHealth = 4;

    [Header("Bonus Health From Items")]
    public int bonusHealthFromItems = 0;

    public PlayerInputActions.PlayerActions playerInputs;
    public PlayerInputActions.UIActions uIInputs;

    [Header("Item Runtime Context")]
    public LayerMask enemyLayer;
    public GameObject paintGlobPrefab;
    public GameObject paintBurstPrefab;
    public GameObject healAuraPrefab;
    public float globSpeed = 18f;
    public int maxJumpCount = 1;
    public int timeToHeal = 5;
    private float healTimer = 0f;

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
        if (health != null)
            health.hurt.AddListener(TookDamage);
    }

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
        {
            RegisterPlayer(player);
            IsReady = true;

            // Start from base health first
            health.maxHealth = startingHealth;
            health.currentHealth = health.maxHealth;

            
            FindObjectOfType<ItemEffectsManager>()?.Reapply();

            
            RecalculateMaxHealth(true);
        }
    }

    void Update()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            InitializeComponents();
            return;
        }
        if (health != null)
        {
            // keep max health synced to current bonus health
            int desiredMax = startingHealth + bonusHealthFromItems;
            if (health.maxHealth != desiredMax)
            {
                int oldMax = health.maxHealth;
                health.maxHealth = desiredMax;

                if (health.currentHealth > health.maxHealth)
                    health.currentHealth = health.maxHealth;
                else if (health.maxHealth > oldMax)
                    health.currentHealth += (health.maxHealth - oldMax);
            }
        }

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

        if (health != null)
        {
            if (health.currentHealth < health.maxHealth)
            {
                healTimer += Time.deltaTime;
                if (healTimer >= timeToHeal)
                {
                    health.Heal(1);
                    healTimer = 0f;
                }
            }
            else
            {
                healTimer = 0f;
            }
        }
    }

    public void RegisterPlayer(GameObject newPlayer)
    {
        player = newPlayer;
        InitializeComponents();

        if (health != null)
        {
            health.hurt.RemoveListener(TookDamage);
            health.hurt.AddListener(TookDamage);
        }

        Debug.Log("Player registered in PlayerManager");
    }

    private void InitializeComponents()
    {
        wallet = GetComponent<Currency>() ?? gameObject.AddComponent<Currency>();
        health = GetComponent<Health>() ?? gameObject.AddComponent<Health>();
        inventory = GetComponent<Inventory>() ?? gameObject.AddComponent<Inventory>();
        stats = GetComponent<PlayerStats>() ?? gameObject.AddComponent<PlayerStats>();
    }

    public void RecalculateMaxHealth(bool refillToFull)
    {
        if (health == null) return;

        health.maxHealth = startingHealth + bonusHealthFromItems;

        if (refillToFull)
            health.currentHealth = health.maxHealth;
        else
            health.currentHealth = Mathf.Clamp(health.currentHealth, 0, health.maxHealth);
    }

    public void SetBonusHealthFromItems(int bonus)
    {
        bonusHealthFromItems = Mathf.Clamp(bonus, 0, 4);
        RecalculateMaxHealth(false);
    }

    public void EditorInit()
    {
        Debug.Log("editorInit");
        player = GameObject.FindGameObjectWithTag("Player");

        if (player != null)
            RegisterPlayer(player);

        IsReady = true;
        RecalculateMaxHealth(true);
    }

    public void Stunned()
    {
        player.GetComponent<PlayerMovement>().Stunned();
        health.Revive(health.maxHealth);
    }

    private void TookDamage()
    {
        healTimer = 0f;
    }

    new void OnDestroy()
    {
        base.OnDestroy();
        playerInputs.Disable();
    }
}