using UnityEngine;

public class GrindRailTrigger : MonoBehaviour
{
    public float tickInterval = 0.6f;
    Coroutine tick;
    void OnTriggerEnter(Collider other){
        if (!other.CompareTag("Player")) return;
        GameEvents.PlayerStartedGrinding?.Invoke();
        if (tick == null) tick = StartCoroutine(Ticker());
    }
    void OnTriggerExit(Collider other){
        if (!other.CompareTag("Player")) return;
        if (tick != null) { StopCoroutine(tick); tick = null; }
    }
    System.Collections.IEnumerator Ticker(){
        while (true){ yield return new WaitForSeconds(tickInterval);
            GameEvents.PlayerGrindingTick?.Invoke();
        }
    }
}
