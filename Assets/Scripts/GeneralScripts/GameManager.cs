
using SuperPupSystems.Helper;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int currentStage;
    public float stageTime;
    public GameObject boss;
    public GameObject stageGate;
    public bool bossDefeated;
    public GameObject shopUi;
    void Start()
    {
        StartStage();
    }

    void Update()
    {
        if (bossDefeated)
        {
            stageGate.SetActive(false);
        }
    }
    public void StartStage()
    {
        GetComponent<Timer>().countDownTime = stageTime;
    }

    public void StageComplete()
    {
        shopUi.SetActive(true);
    }

    public void SpawnBoss()
    {
        GameObject _boss = Instantiate(boss, transform.position, transform.rotation);
        GameObject centerPoint = Instantiate(new GameObject(), transform.position, transform.rotation);
        _boss.GetComponent<Boss>().centerPoint = centerPoint.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        StageComplete();
    }
}
