using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public SprayPaintLine sprayScript;
    
    [Header("UI Components")]
    public Image ammoFillImage;
    public TextMeshProUGUI ammoText;

    public Gradient ammoGradient;

    void Update()
    {
        if (sprayScript == null) return;

        // Update the Fill Amount (expects a value between 0 and 1)
        if (ammoFillImage != null)
        {
            // Use the 0-1 normalized value from your spray script
            ammoFillImage.fillAmount = sprayScript.GetNormalizedAmmo();
            ammoFillImage.color = ammoGradient.Evaluate(sprayScript.GetNormalizedAmmo());
        }

        // Update the Text
        if (ammoText != null)
        {
            ammoText.text = + Mathf.CeilToInt(sprayScript.GetAmmoPercentage()) + "%";
        }
    }
}