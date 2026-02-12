using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaintProgressUI : MonoBehaviour
{
    [Header("References")]
    public Paintable targetPaintable; // Drag the object with Paintable script here
    public Slider progressSlider;
    public TextMeshProUGUI percentText;
    public Slider targetSlider;
    public TextMeshProUGUI targetText;

    [Header("Settings")]
    public bool hideWhenComplete = false;

    void Update()
    {
        if (targetPaintable == null) return;

        // Get the current percentage (0-100)
        float coverage = targetPaintable.percentageCovered;
        float normalizedCoverage = coverage / 100f;

        float targetCoverage = targetPaintable.targetCoverPercent;
        float normalizedTarget = targetCoverage / 100f;

        // Update Slider
        if (progressSlider != null)
        {
            progressSlider.value = normalizedCoverage;
        }

        if (targetSlider != null)
        {
            targetSlider.value = normalizedTarget;
        }

        // Update Text
        if (percentText != null)
        {
            percentText.text = string.Format("{0:0}%", coverage);
        }
        if (targetText != null)
        {
            targetText.text = string.Format("Target: {0:0}%", targetCoverage);
        }

        // Hide UI if target is fully covered
        if (hideWhenComplete && targetPaintable.covered || normalizedTarget == normalizedCoverage)
        {
            gameObject.SetActive(false);
        }
    }
}