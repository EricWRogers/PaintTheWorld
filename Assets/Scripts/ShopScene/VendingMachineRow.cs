using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class VendingMachineRow : MonoBehaviour
{
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text costText;

    public Image highlightBg;      
    public CanvasGroup canvasGroup; 
    public SelectableRow selectable;

    public void Bind(SkillData s, int skillIndex, Currency wallet)
    {
        if (s == null) return;

        if (nameText) nameText.text = s.displayName;
        if (levelText) levelText.text = $"Lv {s.level}/{s.maxLevel}";

        int cost = s.CostForNextLevel();
        bool isMax = cost < 0;

        if (costText)
            costText.text = isMax ? "Maxed" : $"$ {cost}";

       
    }

    public void SetDimmed(bool dim)
    {
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup) canvasGroup.alpha = dim ? 0.45f : 1f;
    }
}