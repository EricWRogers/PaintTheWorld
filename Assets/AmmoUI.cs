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
        RefreshUI();
    }

    private void Update()
    {
        if (sprayScript == null)
        {
            TryAutoAssignSprayScript();
            if (sprayScript == null) return;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
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

        if (PlayerManager.instance != null && PlayerManager.instance.player != null)
        {
            sprayScript = PlayerManager.instance.player.GetComponentInChildren<SprayPaintLine>(true);
        }

        if (sprayScript == null)
        {
            Debug.LogWarning("AmmoUI could not find the live SprayPaintLine on the player.");
        }
        else
        {
            Debug.Log("AmmoUI assigned SprayPaintLine from player: " + sprayScript.name);
        }
    }
}