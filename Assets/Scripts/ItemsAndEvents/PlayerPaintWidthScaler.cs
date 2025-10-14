using UnityEngine;

//attach to same object as painting scripts
public class PlayerPaintWidthScaler : MonoBehaviour
{
    [Tooltip("Multiply your paint width by this value (items modify this at runtime).")]
    public float widthMultiplier = 1f;

    // Call this wherever we compute the brunsh width
    public float Apply(float baseWidth) => baseWidth * Mathf.Max(0.1f, widthMultiplier);
    
}
