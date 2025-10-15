
using UnityEngine;
using SuperPupSystems.Helper;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
public class EnemyHealthRelay : MonoBehaviour
{
    private Health hp;

    private void Awake()
    {
        hp = GetComponent<Health>();
        if (hp.outOfHealth == null) hp.outOfHealth = new UnityEvent();
    }

    private void OnEnable()  => hp.outOfHealth.AddListener(OnOut);
    private void OnDisable() => hp.outOfHealth.RemoveListener(OnOut);

    private void OnOut()
    {
        GameEvents.EnemyKilled?.Invoke(gameObject);
        // Destroy(gameObject);
    }
}
