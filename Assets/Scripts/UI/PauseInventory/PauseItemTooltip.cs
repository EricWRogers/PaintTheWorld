using UnityEngine;
using TMPro;

public class PauseItemTooltip : MonoBehaviour
{
    public GameObject root;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    void Awake()
    {
        Hide();
    }

    public void Show(string itemName, string itemDescription)
    {
        if (root != null) root.SetActive(true);
        if (nameText != null) nameText.text = itemName;
        if (descriptionText != null) descriptionText.text = itemDescription;
    }

    public void Hide()
    {
        if (root != null) root.SetActive(false);
    }
}