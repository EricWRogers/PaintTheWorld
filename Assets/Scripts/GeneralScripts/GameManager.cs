
using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager>
{
    public int currentStage;
    public GameObject boss;
    public GameObject stageGate;
    public bool bossDefeated;
    void Start()
    {
        StartStage();
        
    }

    void Update()
    {
        
    }
    public void StartStage()
    {
        GetComponent<Timer>().StartTimer();
    }

    public void StageComplete()
    {
        SceneManager.LoadSceneAsync("SkillAndShopTest");
    }

    public void SpawnBoss()
    {
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
