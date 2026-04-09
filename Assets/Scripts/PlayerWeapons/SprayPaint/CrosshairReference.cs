using UnityEngine;
using UnityEngine.UI;

public class CrosshairReference : MonoBehaviour
{
    public Image image;

    private void Awake()
    {
        if (image == null)
            image = GetComponent<Image>();
    }
}