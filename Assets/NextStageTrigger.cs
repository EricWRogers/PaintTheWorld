using UnityEngine;

public class NextStageTrigger : MonoBehaviour
{
    public GameObject triggerBlocker;

    void Start()
    {
        GameManager.instance.stageDefeated += DisableBlocker;
    }
    void OnTriggerEnter(Collider other)
    {
        GameManager.instance.StageComplete();
    }

    private void DisableBlocker()
    {
        triggerBlocker.SetActive(false);
        GameManager.instance.stageDefeated -= DisableBlocker;
    }
}
