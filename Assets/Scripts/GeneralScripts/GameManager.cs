
using SuperPupSystems.Helper;
using UnityEngine;

public class GameManager : Singleton<GameManager>
{
    public int currentStage;
    public float stageTime;
    public GameObject Boss;
    void Start()
    {
        StartStage();
    }

    void Update()
    {

    }
    public void StartStage()
    {
        GetComponent<Timer>().countDownTime = stageTime;
    }

    public void StageComplete()
    {
        
    }

    public void SpawnBoss()
    {
        GameObject boss = Instantiate(Boss, transform.position, transform.rotation);
        GameObject centerPoint = Instantiate(new GameObject(), transform.position, transform.rotation);
        boss.GetComponent<Boss>().centerPoint = centerPoint.transform;
    }

    void OnTriggerEnter(Collider other)
    {
        StageComplete();
    }
}
