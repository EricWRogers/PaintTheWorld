using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class KioskListNavigator : MonoBehaviour
{
    public Color normal = Color.white;
    public Color highlight = new Color(1f, 1f, 0.6f);

    public List<SelectableRow> rows = new();
    int index = 0;

    public void SetRows(List<SelectableRow> newRows)
    {
        rows = newRows;
        index = 0;
        RefreshHighlight();
    }

    void Update()
    {
        if (!KioskCameraController.I || !KioskCameraController.I.IsInKiosk) return;

        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) Move(-1);
        if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) Move(+1);

        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (rows.Count > 0) rows[index].TrySelect();
        }
    }

    void Move(int dir)
    {
        if (rows.Count == 0) return;
        index = (index + dir + rows.Count) % rows.Count;
        RefreshHighlight();
    }

    void RefreshHighlight()
    {
        for (int i = 0; i < rows.Count; i++)
            rows[i].SetHighlighted(i == index);
    }
}
