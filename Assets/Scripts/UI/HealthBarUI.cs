using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SuperPupSystems.Helper;

public class HealthBarUI : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Health target;
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;

    [Header("Grow Settings")]
    [SerializeField] private RectTransform widthTarget; 
    [SerializeField] private float basePreferredWidth = 200f; // width at baselineMax
    [SerializeField] private int baselineMax = 100; // your starting maxHealth

    private LayoutElement widthLE;

    private void Awake()
    {
        if (!target) target = FindObjectOfType<Health>();
        if (!widthTarget && slider) widthTarget = slider.GetComponent<RectTransform>();
        if (widthTarget) widthLE = widthTarget.GetComponent<LayoutElement>();
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
        if (target != null)
            OnHealthChanged(new HealthChangedObject { maxHealth = target.maxHealth, currentHealth = target.currentHealth, delta = target.currentHealth });
    }

    private void OnHealthChanged(HealthChangedObject obj)
    {
        // Values
        if (slider)
        {
            slider.maxValue = obj.maxHealth;
            slider.value = obj.currentHealth;
        }
        if (label) label.text = $"{obj.currentHealth}/{obj.maxHealth}";

        // Width scaling
        if (widthTarget)
        {
            float factor = Mathf.Max(0.5f, (float)obj.maxHealth / Mathf.Max(1, baselineMax)); // don’t shrink too tiny
            float w = basePreferredWidth * factor;

            if (widthLE)
            {
                widthLE.preferredWidth = w;
            }
            else
            {
                var rt = widthTarget;
                var sd = rt.sizeDelta;
                sd.x = w;
                rt.sizeDelta = sd;
            }

            
            LayoutRebuilder.ForceRebuildLayoutImmediate(widthTarget);
        }
    }
}
