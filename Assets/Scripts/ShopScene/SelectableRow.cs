using UnityEngine;
using UnityEngine.UI;

public class SelectableRow : MonoBehaviour
{
    public Image highlightBG;
    System.Action onSelect;
    bool interactable = true;

    public void Bind(System.Action onSelect, bool interactable)
    {
        this.onSelect = onSelect;
        this.interactable = interactable;
        SetHighlighted(false);
    }

    public void SetHighlighted(bool on)
    {
        if (!highlightBG) return;
        highlightBG.color = on ? new Color(1f,1f,0.6f) : Color.white;
    }

    public void TrySelect()
    {
        if (!interactable) return;
        onSelect?.Invoke();
    }
}
