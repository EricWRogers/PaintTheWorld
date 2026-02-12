using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaintProgressUI : MonoBehaviour
{
    [Header("References")]
    public Paintable targetPaintable; 
    public Slider progressSlider;
    public TextMeshProUGUI percentText;

    [Header("Settings")]
    public bool hideWhenComplete = false;

    void Update()
    {
        if (targetPaintable == null) return;

        // Get the current percentage (0-100)
        float coverage = targetPaintable.percentageCovered;
        float normalizedCoverage = coverage / 100f;

        // Update Slider
        if (progressSlider != null)
        {
            progressSlider.value = normalizedCoverage;
        }
        // Update Text
        if (percentText != null)
        {
            percentText.text = string.Format("{0:0}%", coverage);
        }

        // Hide UI if target is fully covered
        if (hideWhenComplete && targetPaintable.covered)
        {
            gameObject.SetActive(false);
        }
    }
}