using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleCardRow : MonoBehaviour
{
    public Image icon;
    public TMP_Text nameText;
    public Button actionButton;
    public TMP_Text actionLabel;
    [HideInInspector] public int index;
}

