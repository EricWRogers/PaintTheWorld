using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public SprayPaintLine sprayScript;

    [Header("UI Components")]
    public Slider ammoSlider;
    public Image sliderFillImage;

    [Header("Visuals")]
    public Gradient ammoGradient;

    private void Start()
    {
        TryAutoAssignSprayScript();
    }

    private void Update()
    {
        if (sprayScript == null)
        {
            TryAutoAssignSprayScript();
            if (sprayScript == null) return;
        }

        float normalizedAmmo = sprayScript.GetNormalizedAmmo();

        if (ammoSlider != null)
        {
            ammoSlider.value = normalizedAmmo;
        }

        if (sliderFillImage != null)
        {
            sliderFillImage.color = ammoGradient.Evaluate(normalizedAmmo);
        }
    }

    private void TryAutoAssignSprayScript()
    {
        if (sprayScript != null)
            return;

        sprayScript = FindObjectOfType<SprayPaintLine>(true);

        if (sprayScript != null)
        {
            Debug.Log("AmmoUI auto-assigned SprayPaintLine: " + sprayScript.name);
        }
        else
        {
            Debug.LogWarning("AmmoUI could not find SprayPaintLine in the scene.");
        }
    }
}