using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    public SprayPaintLine sprayScript;
    public Slider ammoSlider;
    public TextMeshProUGUI ammoText;

    void Update()
    {
        if (sprayScript == null) return;

        // Update the Slider (needs 0 to 1)
        if (ammoSlider != null)
        {
            ammoSlider.value = sprayScript.GetNormalizedAmmo();
        }

        // Update the Text (needs 0 to 100)
        if (ammoText != null)
        {
            ammoText.text = "Ammo: " + Mathf.CeilToInt(sprayScript.GetAmmoPercentage()) + "%";
        }
    }
}