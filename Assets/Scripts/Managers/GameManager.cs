
using KinematicCharacterControler;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Timer))]
public class GameManager : SceneAwareSingleton<GameManager>
{
    public int currentWave;
    public int enemyAmount = 30;
    public int totalEnemyKills;
    public int currKilledInWave;

    [Header("CombatCycle")]
    public float firstWaveTimer;
    public float prepTimer;
    public bool inWave;
    public float timer;
    

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
        currentWave = 0;
        inWave = false;
        timer = firstWaveTimer;
        RecalculateScaling();
        gameplayStarted = false;
        IsReady = true;
    }

    void Update()
    {
        if (!inWave)
        {
            timer -= Time.deltaTime;
            if (timer < 0)
            {
                StartWave();
            }
        }

        if(currKilledInWave >= EnemyManager.instance.spawnAmount && inWave)
        {
            WaveComplete();
        }

    }
    public void StartWave()
    {
        Debug.Log(Mathf.RoundToInt(enemyAmount * EnemyCountModifier));
        inWave = true;
        currentWave++;
        EnemyManager.instance.spawnAmount = Mathf.RoundToInt(enemyAmount * EnemyCountModifier);
    }
    
    public void WaveComplete()
    {
        Debug.Log("wave complete");
        timer = prepTimer;
        inWave = false;
        currKilledInWave = 0;
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
    }
    
    public void ResumeGame()
    {
         Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
