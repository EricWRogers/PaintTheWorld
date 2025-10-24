
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillsHorizontalUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform contentParent;   // ScrollView/Viewport/Content
    public SimpleCardRow cardPrefab;      // Card_Simple prefab

    private readonly List<SimpleCardRow> pool = new();
    private PlayerManager pm;

    void OnEnable()
    {
        pm = PlayerManager.instance;
        BuildPool(pm?.stats?.skills.Count ?? 0);
        RefreshAll();

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }

    void BuildPool(int needed)
    {
        if (!contentParent || !cardPrefab) return;

        for (int i = pool.Count; i < needed; i++)
        {
            var card = Instantiate(cardPrefab, contentParent);
            Wire(card);
            pool.Add(card);
        }
        for (int i = needed; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);
    }

    void Wire(SimpleCardRow card)
    {
        card.actionButton.onClick.RemoveAllListeners();
        card.actionButton.onClick.AddListener(() => Upgrade(card.index));
    }

    void Upgrade(int index)
    {
        pm = PlayerManager.instance;
        if (!pm || pm.stats == null || pm.wallet == null) return;

        if (pm.stats.TryUpgradeSkill(index, pm.wallet))
            FindObjectOfType<ItemEffectsManager>()?.Reapply();

        RefreshRow(index);
    }

    public void RefreshAll()
    {
        pm = PlayerManager.instance;
        if (pm?.stats == null) return;

        var skills = pm.stats.skills;
        BuildPool(skills.Count);

        for (int i = 0; i < skills.Count; i++)
        {
            var s = skills[i];
            var card = pool[i];
            card.index = i;
            card.gameObject.SetActive(true);

            if (card.icon)      card.icon.sprite = s.icon;
            if (card.nameText)  card.nameText.text = s.displayName;

            int cost = s.CostForNextLevel();
            bool isMax = cost < 0;

            if (card.actionLabel) card.actionLabel.text = isMax ? "Maxed" : $"$ {cost}";
            if (card.actionButton) card.actionButton.interactable = !isMax && pm.wallet.amount >= cost;
        }

        // hide any extra pooled cards if list shrank
        for (int i = skills.Count; i < pool.Count; i++) pool[i].gameObject.SetActive(false);
    }

    void RefreshRow(int i)
    {
        pm = PlayerManager.instance;
        if (pm?.stats == null) return;
        var s = pm.stats.GetSkill(i);
        if (s == null || i < 0 || i >= pool.Count) return;

        var card = pool[i];
        if (card.icon)      card.icon.sprite = s.icon;
        if (card.nameText)  card.nameText.text = s.displayName;

        int cost = s.CostForNextLevel();
        bool isMax = cost < 0;
        if (card.actionLabel) card.actionLabel.text = isMax ? "Maxed" : $"$ {cost}";
        if (card.actionButton) card.actionButton.interactable = !isMax && pm.wallet.amount >= cost;
    }
}
