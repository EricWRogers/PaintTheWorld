
using KinematicCharacterControler;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
public class GameManager : SceneAwareSingleton<GameManager>
{
    public int currentStage;
    public GameObject boss;
    public GameObject stageGate;
    public UnityAction bossDefeated;

    private bool gameplayStarted = false;

    public override void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        gameplayStarted = false;
        IsReady = true;
    }

    public void BeginGameplay()
    {
        if (gameplayStarted) return;
        gameplayStarted = true;

        EnemyManager.instance.SpawnEnemies(EnemyManager.instance.maxNumberOfEnemies / 4);

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
    public void EditorInit()
    {
        IsReady = true; // mark ready
    }
}
