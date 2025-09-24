
using System.Collections;
using UnityEngine;
using SuperPupSystems.Helper;

[RequireComponent(typeof(SphereCollider))]
public class HealAura : MonoBehaviour
{
    public float radius = 2f;
    public float duration = 6f;
    public int healPerTick = 1;
    public float tickInterval = 0.3f;

    [HideInInspector] public Transform player;
    [HideInInspector] public Health playerHealth;

    private SphereCollider col;

    private void Awake()
    {
        col = GetComponent<SphereCollider>();
        col.isTrigger = true;
    }

    private void OnEnable()
    {
        col.radius = radius;
        StartCoroutine(Run());
    }

    private IEnumerator Run()
    {
        float time = 0f;
        while (time < duration)
        {
            time += tickInterval;

            if (player && playerHealth && playerHealth.currentHealth > 0)
            {
                if (Vector3.Distance(transform.position, player.position) <= radius + 0.05f)
                {
                    playerHealth.Heal(healPerTick);
                    // PlayerHealthRelay will fire PlayerHealed for us via healthChanged delta
                    
                    GameEvents.PlayerHealed?.Invoke(healPerTick);
                }
            }
            yield return new WaitForSeconds(tickInterval);
        }
        Destroy(gameObject);
    }
}
