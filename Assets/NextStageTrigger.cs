using UnityEngine;

public class NextStageTrigger : MonoBehaviour
{
    public GameObject triggerBlocker;

    void Start()
    {
        GameManager.instance.bossDefeated += DisableBlocker;
    }
    void OnTriggerEnter(Collider other)
    {
        GameManager.instance.StageComplete();
    }

    private void DisableBlocker()
    {
        triggerBlocker.SetActive(false);
        GameManager.instance.bossDefeated -= DisableBlocker;
    }
}
