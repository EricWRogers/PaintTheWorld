using UnityEngine;
using UnityEngine.UI;
using System;

public class SelectableRow : MonoBehaviour
{
    public Image highlightBG;
    private Action onConfirm;
    private bool selectable = true;

    public void Bind(Action confirmAction, bool canSelect = true)
    {
        onConfirm = confirmAction;
        selectable = canSelect;
        SetHighlighted(false);
    }

    public void SetHighlighted(bool on)
    {
        if (highlightBG) highlightBG.enabled = on;
    }

    public void Confirm()
    {
        if (!selectable) return;
        onConfirm?.Invoke();
    }
}
