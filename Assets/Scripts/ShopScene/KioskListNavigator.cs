using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class KioskListNavigator : MonoBehaviour
{
    public List<SelectableRow> rows = new();
    public int index = 0;
    public bool active;

    [Header("Optional Scroll Support")]
    public ScrollRect scrollRect;          
    public RectTransform content;          
    public RectTransform viewport;          

    public void SetRows(List<SelectableRow> newRows)
    {
        rows = newRows;
        index = Mathf.Clamp(index, 0, rows.Count - 1);
        RefreshHighlight();
        ScrollToIndex();
    }

    void Update()
    {
        if (!active) return;
        if (rows == null || rows.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            index = (index - 1 + rows.Count) % rows.Count;
            RefreshHighlight();
            ScrollToIndex();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            index = (index + 1) % rows.Count;
            RefreshHighlight();
            ScrollToIndex();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            rows[index].Confirm();
        }
    }

    void RefreshHighlight()
    {
        for (int i = 0; i < rows.Count; i++)
            rows[i].SetHighlighted(i == index);
    }

    void ScrollToIndex()
    {
        if (!scrollRect || rows == null || rows.Count == 0) return;
        if (!content || !viewport) return;

       
        float t = (rows.Count <= 1) ? 1f : 1f - (index / (float)(rows.Count - 1));
        scrollRect.verticalNormalizedPosition = Mathf.Clamp01(t);
    }
}