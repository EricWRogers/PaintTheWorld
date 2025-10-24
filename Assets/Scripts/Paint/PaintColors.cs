using System.Collections.Generic;
using UnityEngine;

public class PaintColors : MonoBehaviour
{
    [Tooltip("color one Damage over time. Enemy more damage")]
    public Color damagePaint;
    [Tooltip("color two Jumping. Enemy Stun")]
    public Color jumpPaint;
    [Tooltip("color three Movement speed. Enemy up attack speed")]
    public Color movementPaint;

    public List<Color> colorDict;
}
