using UnityEngine;
using SuperPupSystems.Helper;

[DisallowMultipleComponent]
public class OvershieldController : MonoBehaviour
{
    public int Capacity { get; set; } = 0;   // max shield HP
    public int Current { get; set; } = 0;    // current shield HP

    private Health health;

    void Awake()
    {
        health = GetComponent<Health>();
    }

    void OnEnable()
    {
    
        GameEvents.PlayerDamaged += OnPlayerDamaged;


    }

    void OnDisable()
    {
        GameEvents.PlayerDamaged -= OnPlayerDamaged;
    }

    public void RaiseUI() => OvershieldUIEvent?.Invoke(Capacity, Current);

   
    public static System.Action<int,int> OvershieldUIEvent;

   
    private void OnPlayerDamaged(int amount)
    {
        if (!health || amount <= 0) return;
        if (Current <= 0) { RaiseUI(); return; }

        int soak = Mathf.Min(Current, amount);
        Current -= soak;

        // Heal back the soaked portion
        if (health.currentHealth > 0)
        {
            health.Heal(soak);
        }
        else
        {
            // bring back to (at least) 1 HP, then top up with remaining soak
            int restore = Mathf.Max(1, soak);
            health.Revive(restore);
        }

        RaiseUI();
    }
}

