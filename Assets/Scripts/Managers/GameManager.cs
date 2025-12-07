
using KinematicCharacterControler;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Timer))]
public class GameManager : SceneAwareSingleton<GameManager>
{
    public int currentWave;
    public int totalWave;
    public int enemyAmount = 30;
    public int totalEnemyKills;
    public int currKilledInWave;
    public GameObject pauseMenu;

    [Header("CombatCycle")]
    public float firstWaveTimer;
    public float prepTimer;
    public bool inWave;
    public Timer timer;
    

    [Header("Scaling Settings")]
    public EnemyScalingData scalingData;

    // These are modifiers applied to base stats.
    private float enemyDamageModifier;
    private float enemyHealthModifier;
    private float bossHealthModifier;
    private float enemyCountModifier;
    private float coinGainModifier;
    private bool gameplayStarted = false;

    public bool m_isPaused;

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if(EnemyManager.instance.spawnerAreas.Count != 0)
        {
            currentWave = 0;
            inWave = false;
            timer.StartTimer(firstWaveTimer);
            RecalculateScaling();
        }
        
        gameplayStarted = false;
        IsReady = true;
    }

    void Update()
    {
        if(currKilledInWave >= Mathf.RoundToInt(enemyAmount * EnemyCountModifier) && inWave)
        {
            WaveComplete();
        }

    }
    public void StartWave()
    {
        Debug.Log("start wave " +Mathf.RoundToInt(enemyAmount * EnemyCountModifier));
        inWave = true;
        currentWave++;
        EnemyManager.instance.spawnAmount = Mathf.RoundToInt(enemyAmount * EnemyCountModifier);
    }
    
    public void WaveComplete()
    {
        if(currentWave == totalWave)
        {
            ResetGame();
            SceneManager.LoadScene("MainMenu");
        }
        timer.StartTimer(prepTimer);
        inWave = false;
        currKilledInWave = 0;
        RecalculateScaling();
        EnemyManager.instance.ResetWave();
        EnemyManager.instance.ChooseSpawnArea();
    }

    public void BeginGameplay()
    {
        if (gameplayStarted) return;
        gameplayStarted = true;
        EnemyManager.instance.ChooseSpawnArea();
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

    private void RecalculateScaling()
    {
        if (scalingData == null)
        {
            Debug.LogWarning("No EnemyScalingData assigned!");
            return;
        }

        // Base modifiers from stage
        enemyDamageModifier = 1f + (scalingData.enemyDamageIncrease * (currentWave - 1));
        enemyHealthModifier = 1f + (scalingData.enemyHealthIncrease * (currentWave - 1));
        bossHealthModifier = 1f + (scalingData.bossHealthIncrease * (currentWave - 1));
        enemyCountModifier = 1f + (scalingData.enemyCountIncrease * currentWave);
        coinGainModifier = 1f + (scalingData.coinGainIncrease * (currentWave - 1));
    }

    public void ResetGame()
    {
        enemyDamageModifier = 0;
        enemyHealthModifier = 0;
        bossHealthModifier = 0;
        enemyCountModifier = 0;
        coinGainModifier = 0;
        currentWave = 0;
        totalEnemyKills = 0;
        PlayerManager.instance.ResetData();
    }

    // Public accessors for modifiers
    public float EnemyDamageModifier => enemyDamageModifier;
    public float EnemyHealthModifier => enemyHealthModifier;
    public float BossHealthModifier => bossHealthModifier;
    public float EnemyCountModifier => enemyCountModifier;
    public float CoinGainModifier => coinGainModifier;
    public void EditorInit()
    {
        IsReady = true;
    }

    
}
