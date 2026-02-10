using System.Collections.Generic;
using UnityEngine;

public class KioskListNavigator : MonoBehaviour
{
    public List<SelectableRow> rows = new();
    public int index = 0;
    public bool active;

    public void SetRows(List<SelectableRow> newRows)
    {
        rows = newRows;
        index = Mathf.Clamp(index, 0, rows.Count - 1);
        RefreshHighlight();
    }

    void Update()
    {
        if (!active) return;
        if (rows == null || rows.Count == 0) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            index = (index - 1 + rows.Count) % rows.Count;
            RefreshHighlight();
        }
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            index = (index + 1) % rows.Count;
            RefreshHighlight();
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
}
