using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class PaintColorIndicator : MonoBehaviour
{
    public GetPaintColor provider;  
    public float lerpSpeed = 12f;   // how fast the UI blends to new color
    public bool showAlpha = true;   // if false, clamp alpha to 1

    private Image img;
    private Color target;

    void Awake()
    {
        img = GetComponent<Image>();
        if (!provider) provider = FindObjectOfType<GetPaintColor>();
        target = img ? img.color : Color.white;
    }

    void Update()
    {
        if (provider != null)
        {
            target = provider.standingColor;
            if (!showAlpha) target.a = 1f;
        }

        if (img != null)
        {
            img.color = Color.Lerp(img.color, target, Time.deltaTime * lerpSpeed);
        }
    }
}

