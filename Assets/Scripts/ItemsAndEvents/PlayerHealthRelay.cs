
using UnityEngine;
using SuperPupSystems.Helper;
using UnityEngine.Events;

[RequireComponent(typeof(Health))]
public class PlayerHealthRelay : MonoBehaviour
{
    private Health hp;

    private void Awake()
    {
        hp = GetComponent<Health>();
        if (hp.healthChanged == null) hp.healthChanged = new HealthChangedEvent();
        if (hp.healed == null) hp.healed = new HealedEvent();
        if (hp.hurt == null) hp.hurt = new UnityEvent();
        if (hp.outOfHealth == null) hp.outOfHealth = new UnityEvent();
    }

    private void OnEnable()
    {
        hp.healthChanged.AddListener(OnHealthChanged);
        hp.healed.AddListener(OnHealed);
        hp.hurt.AddListener(OnHurt);
    }

    private void OnDisable()
    {
        hp.healthChanged.RemoveListener(OnHealthChanged);
        hp.healed.RemoveListener(OnHealed);
        hp.hurt.RemoveListener(OnHurt);
    }

    private void OnHealthChanged(HealthChangedObject obj)
    {
        // obj.delta < 0 means the player lost health; positive means healed
        if (obj.delta < 0) GameEvents.PlayerDamaged?.Invoke(-obj.delta);
        else if (obj.delta > 0) GameEvents.PlayerHealed?.Invoke(obj.delta);
    }

    private void OnHealed(int amount)
    {
        
        GameEvents.PlayerHealed?.Invoke(amount);
    }

    private void OnHurt()
    {
        //already handled
    }
}

