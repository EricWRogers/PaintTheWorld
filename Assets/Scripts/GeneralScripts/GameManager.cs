
using KinematicCharacterControler;
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public int currentStage;
    public GameObject boss;
    public GameObject stageGate;
    public bool bossDefeated;
    private bool m_timerStarted;
    void Start()
    {
        StartStage();

    }

    void Update()
    {
        if (SceneManager.GetActiveScene().name != "SkillsAndShopTest" && !m_timerStarted)
        {
            StartStage();
        }
    }
    public void StartStage()
    {
        GetComponent<Timer>().StartTimer();
        m_timerStarted = true;
    }

    public void StageComplete()
    {
        PlayerManager.instance.player.GetComponent<PlayerMovement>().lockCursor = false;
        PlayerManager.instance.player.GetComponent<PlayerMovement>().HandleCursor();
        EnemyManager.instance.ClearSpawners();
        SceneManager.LoadSceneAsync(1);
        m_timerStarted = false;
    }

    public void SpawnBoss()
    {
        GetComponent<Timer>().StopTimer();
        GameObject _boss = Instantiate(boss, transform.position, transform.rotation);
        GameObject centerPoint = Instantiate(new GameObject(), transform.position, transform.rotation);
        _boss.GetComponent<Boss>().centerPoint = centerPoint.transform;
        _boss.GetComponent<Health>().outOfHealth.AddListener(BossDefeated);
    }
    public void BossDefeated()
    {
        stageGate.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        StageComplete();
    }
    
}
