
using KinematicCharacterControler;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(Timer))]
public class GameManager : SceneAwareSingleton<GameManager>
{
    public GameObject boss;
    public UnityAction bossDefeated;
    public float stageTimer;
    public int stagesBeforeFinal = 5;
    public bool allowLooping = true;

    [Header("Scaling Settings")]
    public EnemyScalingData scalingData;

    public int CurrentStage { get; private set; } = 1;
    public int CurrentLoop { get; private set; } = 0;

    // These are modifiers applied to base stats.
    private float enemyDamageModifier;
    private float bossHealthModifier;
    private float enemyCountModifier;

    private bool gameplayStarted = false;

    private Timer m_spawnTimer;

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        m_spawnTimer = GetComponent<Timer>();
        m_spawnTimer.StartTimer(stageTimer, true);
        m_spawnTimer.timeout.RemoveAllListeners();
        m_spawnTimer.timeout.AddListener(SpawnBoss);
        RecalculateScaling();
        gameplayStarted = false;
        IsReady = true;
    }

    public void BeginGameplay()
    {
        if (gameplayStarted) return;
        gameplayStarted = true;

        EnemyManager.instance.SpawnEnemies(EnemyManager.instance.baseMaxEnemyCount / 4);

        Timer timer = GetComponent<Timer>();
        if (timer != null)
            timer.StartTimer();
    }

    public void StageComplete()
    {
        GameObject player = PlayerManager.instance.player;
        if (player != null)
        {
            PlayerMovement movement = player.GetComponent<PlayerMovement>();
            movement.lockCursor = false;
            movement.HandleCursor();
        }
        NextStage();
        SceneManager.LoadSceneAsync(1); // later will be either store scene or next stage depending on where you are calling it from
    }

    public void SpawnBoss()
    {
        if (!boss) return;

        GetComponent<Timer>()?.StopTimer();

        GameObject _boss = Instantiate(boss, transform.position, transform.rotation);
        GameObject centerPoint = new GameObject("BossCenterPoint");
        centerPoint.transform.position = transform.position;

        Health bossHealth = _boss.GetComponent<Health>();
        Boss bossScript = _boss.GetComponent<Boss>();
        if (bossScript != null) bossScript.centerPoint = centerPoint.transform;
        if (bossHealth != null) bossHealth.outOfHealth.AddListener(BossDefeated);
    }

    public void BossDefeated()
    {
        bossDefeated.Invoke();
    }
    public void NextStage()
    {
        CurrentStage++;
        RecalculateScaling();
    }

    public void LoopGame()
    {
        CurrentStage = 1;
        CurrentLoop++;
        RecalculateScaling();
    }

    private void RecalculateScaling()
    {
        if (scalingData == null)
        {
            Debug.LogWarning("No EnemyScalingData assigned!");
            return;
        }

        // Base modifiers from stage
        enemyDamageModifier = 1f + (scalingData.enemyDamageIncrease * (CurrentStage - 1));
        bossHealthModifier = 1f + (scalingData.bossHealthIncrease * (CurrentStage - 1));
        enemyCountModifier = 1f + (scalingData.enemyCountIncrease * (CurrentStage - 1));

        // Loop bonuses applied multiplicatively
        if (CurrentLoop > 0)
        {
            enemyDamageModifier *= 1f + (scalingData.loopDamageBonus * CurrentLoop);
            bossHealthModifier *= 1f + (scalingData.loopBossBonus * CurrentLoop);
            enemyCountModifier *= 1f + (scalingData.loopEnemyCountBonus * CurrentLoop);
        }

        Debug.Log($"[Scaling] Stage {CurrentStage} (Loop {CurrentLoop}) => " +
                  $"Damage x{enemyDamageModifier:F2}, Boss HP x{bossHealthModifier:F2}, Count x{enemyCountModifier:F2}");
    }

    // Public accessors for modifiers
    public float EnemyDamageModifier => enemyDamageModifier;
    public float BossHealthModifier => bossHealthModifier;
    public float EnemyCountModifier => enemyCountModifier;
    public void EditorInit()
    {
        m_spawnTimer = GetComponent<Timer>();
        m_spawnTimer.StartTimer(stageTimer, true);
        m_spawnTimer.timeout.RemoveAllListeners();
        m_spawnTimer.timeout.AddListener(SpawnBoss);
        IsReady = true;
    }
}
