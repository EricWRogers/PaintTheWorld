using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using SuperPupSystems.Helper;

public class HealthBarUI : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Health target;     // Assign player's health
    [SerializeField] private Slider slider;     // UI Slider
    [SerializeField] private TMP_Text label;    //possible text to accompany slider

    private void Awake()
    {
        
    }

    private void OnEnable()
    {
        if (target != null)
            target.healthChanged.AddListener(OnHealthChanged);
    }

    private void OnDisable()
    {
        if (target != null)
            target.healthChanged.RemoveListener(OnHealthChanged);
    }

    private void Start()
    {
        // Initialize UI to thee current values
        if (target != null)
            OnHealthChanged(new HealthChangedObject { maxHealth = target.maxHealth, currentHealth = target.currentHealth, delta = target.currentHealth });
    }
    void Update()
    {
        if(target == null)
        {
            target = PlayerManager.instance.health;
            target.healthChanged.AddListener(OnHealthChanged);
            OnHealthChanged(new HealthChangedObject { maxHealth = target.maxHealth, currentHealth = target.currentHealth, delta = target.currentHealth });
        }
    }

    private void OnHealthChanged(HealthChangedObject obj)
    {
        if (slider)
        {
            slider.maxValue = obj.maxHealth;
            slider.value = obj.currentHealth;
        }

        if (label)
            label.text = $"{obj.currentHealth}/{obj.maxHealth}";
    }
}
