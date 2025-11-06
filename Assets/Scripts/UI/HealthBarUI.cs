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
    [SerializeField] private float basePreferredWidth = 200f;
    [SerializeField] private int baselineMax = 100;

    [Header("Overshield Tail")]
    [SerializeField] private RectTransform overshieldTail;  // the extra right-end segment
    [SerializeField] private float tailMaxWidthAtBaseline = 80f; // how wide tail is when shield == baselineMax

    private LayoutElement widthLE;

    void Awake()
    {
        if (!target) target = FindObjectOfType<Health>();
        if (!widthTarget && slider) widthTarget = slider.GetComponent<RectTransform>();
        if (widthTarget) widthLE = widthTarget.GetComponent<LayoutElement>();
    }

    void OnEnable()
    {
        if (target != null)
            target.healthChanged.AddListener(OnHealthChanged);

        OvershieldController.OvershieldUIEvent += OnOvershieldChanged;
    }

    void OnDisable()
    {
        if (target != null)
            target.healthChanged.RemoveListener(OnHealthChanged);

        OvershieldController.OvershieldUIEvent -= OnOvershieldChanged;
    }

    void Start()
    {
        if (target != null)
            OnHealthChanged(new HealthChangedObject { maxHealth = target.maxHealth, currentHealth = target.currentHealth, delta = target.currentHealth });

        // Initialize tail
        OnOvershieldChanged(0, 0);
    }

    void OnHealthChanged(HealthChangedObject obj)
    {
        if (slider)
        {
            slider.maxValue = obj.maxHealth;
            slider.value   = obj.currentHealth;
        }
        if (label) label.text = $"{obj.currentHealth}/{obj.maxHealth}";

       
        if (widthTarget)
        {
            float factor = Mathf.Max(0.5f, (float)obj.maxHealth / Mathf.Max(1, baselineMax));
            float w = basePreferredWidth * factor;

            if (widthLE) widthLE.preferredWidth = w;
            else
            {
                var sd = widthTarget.sizeDelta;
                sd.x = w;
                widthTarget.sizeDelta = sd;
            }
            LayoutRebuilder.ForceRebuildLayoutImmediate(widthTarget);
        }
    }

    void OnOvershieldChanged(int capacity, int current)
    {
        if (!overshieldTail) return;

      
        float capFactor = (float)capacity / Mathf.Max(1, baselineMax);
        float maxTail = tailMaxWidthAtBaseline * capFactor;

       
        float pct = (capacity > 0) ? (float)current / capacity : 0f;
        float tailWidth = maxTail * pct;

        var sd = overshieldTail.sizeDelta;
        sd.x = tailWidth;
        overshieldTail.sizeDelta = sd;

        overshieldTail.gameObject.SetActive(tailWidth > 0.5f);
    }
}
