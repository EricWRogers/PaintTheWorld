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

    void Update()
    {
        if (sprayScript == null) return;

        float normalizedAmmo = sprayScript.GetNormalizedAmmo();

        // Update the Slider Value (0 to 1)
        if (ammoSlider != null)
        {
            ammoSlider.value = normalizedAmmo;
        }

        // Update the Color of the Fill area
        if (sliderFillImage != null)
        {
            sliderFillImage.color = ammoGradient.Evaluate(normalizedAmmo);
        }
    }
}