using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PaintProgressUI : MonoBehaviour
{
    [Header("References")]
    public PaintingObj targetPaintable; 
    public Slider progressSlider;

    [Header("Settings")]
    public bool hideWhenComplete = false;

    void Start()
    {
        progressSlider.maxValue = targetPaintable.targetCoverPercent;
    }

    void Update()
    {
        if (targetPaintable == null) return;

        // Get the current percentage (0-100)
        float coverage = targetPaintable.percentageCovered;

        // Update Slider
        if (progressSlider != null)
        {
            progressSlider.value = coverage;
        }
        // Update Text
        // if (percentText != null)
        // {
        //     percentText.text = string.Format("{0:0}%", coverage);
        // }

        // Hide UI if target is fully covered
        if (hideWhenComplete && targetPaintable.covered)
        {
            gameObject.SetActive(false);
        }
    }
}