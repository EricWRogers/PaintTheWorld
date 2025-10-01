using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PaintSwatchUI : MonoBehaviour
{
    [Tooltip("The component that holds the player's selected paint color.")]
    public PlayerPaint playerPaint;

    [Tooltip("How quickly the UI blends to the new color.")]
    public float lerpSpeed = 12f;

    [Tooltip("Force alpha to 1 so the swatch never disappears.")]
    public bool clampAlphaToOne = true;

    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();
        if (!playerPaint) playerPaint = FindObjectOfType<PlayerPaint>();
    }

    void Update()
    {
        if (!img || !playerPaint) return;

        Color c = playerPaint.selectedPaint;
        if (clampAlphaToOne) c.a = 1f;

        img.color = Color.Lerp(img.color, c, Time.deltaTime * Mathf.Max(1f, lerpSpeed));
    }
}
