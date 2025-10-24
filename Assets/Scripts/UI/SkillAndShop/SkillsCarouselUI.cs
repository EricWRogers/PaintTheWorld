// SkillsCarouselUI.cs
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; 

public class SkillsCarouselUI : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform content;   // SkillsSection/.../Viewport/Content
    public CardUI cardPrefab;       // Card_ItemOrSkill

    private readonly List<CardUI> pool = new();
    PlayerManager pm;

    void OnEnable()
    {
        pm = PlayerManager.instance;
        BuildOrResizePool(pm?.stats?.skills.Count ?? 0);
        RefreshAll();

        Canvas.ForceUpdateCanvases();
        if (content) LayoutRebuilder.ForceRebuildLayoutImmediate(content);
    }

    void BuildOrResizePool(int needed)
    {
        if (!content || !cardPrefab) return;
        for (int i = pool.Count; i < needed; i++)
        {
            var card = Instantiate(cardPrefab, content);
            WireSkillCard(card);
            pool.Add(card);
        }
        for (int i = needed; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);
    }

    void WireSkillCard(CardUI card)
    {
        card.actionButton.onClick.RemoveAllListeners();
        card.actionButton.onClick.AddListener(() => Upgrade(card.dataIndex));
    }

    void Upgrade(int index)
    {
        pm = PlayerManager.instance;
        if (!pm || pm.stats == null) return;

       
        bool ok = pm.stats.TryUpgradeSkill(index, pm.wallet);
        if (ok) FindObjectOfType<ItemEffectsManager>()?.Reapply();

        RefreshAll();
    }

    public void RefreshAll()
    {
        pm = PlayerManager.instance;
        if (pm?.stats == null) return;

        var list = pm.stats.skills;
        for (int i = 0; i < pool.Count; i++)
        {
            var card = pool[i];
            if (i >= list.Count) { card.gameObject.SetActive(false); continue; }

            var s = list[i];
            card.dataIndex = i;
            card.gameObject.SetActive(true);

            if (card.icon)        card.icon.sprite = s.icon;
            if (card.nameText)    card.nameText.text = s.displayName;
            if (card.actionLabel) card.actionLabel.text = "Upgrade";

            int cost = s.CostForNextLevel();       
            bool isMax = cost < 0;
            if (card.priceOrCost)  card.priceOrCost.text = isMax ? "Maxed" : $"Cost: {cost}";
            if (card.stockOrLevel) card.stockOrLevel.text = $"Lv {s.level}/{s.maxLevel}";

            bool canAfford = !isMax && pm.wallet && pm.wallet.amount >= cost;
            if (card.actionButton) card.actionButton.interactable = canAfford;
        }
    }
}
