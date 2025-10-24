using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillsHorizontalUI : MonoBehaviour
{
    [Header("Refs (auto if left empty)")]
    public PlayerStats stats;
    public Currency wallet;

    [Header("UI")]
    public RectTransform contentParent;   
    public SimpleCardRow cardPrefab;     

    private readonly List<SimpleCardRow> pool = new();
    private bool wired = false;

    void Awake()
    {
        ResolveRefs();
    }

    void OnEnable()
    {
        ResolveRefs();
        WireEvents();
        BuildPool(stats?.skills.Count ?? 0);
        RefreshAll();

        Canvas.ForceUpdateCanvases();
        if (contentParent) LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }

    void OnDisable()
    {
        UnwireEvents();
    }

    void ResolveRefs()
    {
        if (!stats)  stats  = PlayerManager.instance ? PlayerManager.instance.stats  : null;
        if (!wallet) wallet = PlayerManager.instance ? PlayerManager.instance.wallet : null;
    }

    void WireEvents()
    {
        if (wired) return;
        if (stats)  stats.onSkillChanged.AddListener(OnSkillChanged);
        if (wallet && wallet.changed != null) wallet.changed.AddListener(OnWalletChanged);
        wired = true;
    }

    void UnwireEvents()
    {
        if (!wired) return;
        if (stats)  stats.onSkillChanged.RemoveListener(OnSkillChanged);
        if (wallet && wallet.changed != null) wallet.changed.RemoveListener(OnWalletChanged);
        wired = false;
    }

    void OnSkillChanged(int index, SkillData data) => RefreshRow(index);
    void OnWalletChanged(int _) => RefreshAll();

    void BuildPool(int needed)
    {
        if (!contentParent || !cardPrefab) return;

        for (int i = pool.Count; i < needed; i++)
        {
            var card = Instantiate(cardPrefab, contentParent);
            WireCard(card);
            pool.Add(card);
        }
        for (int i = needed; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);
    }

    void WireCard(SimpleCardRow card)
    {
        card.actionButton.onClick.RemoveAllListeners();
        card.actionButton.onClick.AddListener(() => TryUpgrade(card.index));
    }

    void TryUpgrade(int index)
    {
        if (!stats || !wallet) return;
        if (stats.TryUpgradeSkill(index, wallet))
        {
            
            FindObjectOfType<ItemEffectsManager>()?.Reapply();
        }
        RefreshRow(index);
    }

    public void RefreshAll()
    {
        ResolveRefs();
        if (stats == null || stats.skills == null) { HideAll(); return; }

        BuildPool(stats.skills.Count);

        for (int i = 0; i < stats.skills.Count; i++)
            RefreshRow(i);

        for (int i = stats.skills.Count; i < pool.Count; i++)
            pool[i].gameObject.SetActive(false);

        Canvas.ForceUpdateCanvases();
        if (contentParent) LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent);
    }

    void HideAll()
    {
        for (int i = 0; i < pool.Count; i++)
            if (pool[i]) pool[i].gameObject.SetActive(false);
    }

    void RefreshRow(int i)
    {
        if (stats == null || stats.skills == null) return;
        if (i < 0 || i >= stats.skills.Count) return;
        if (i >= pool.Count) return;

        var s = stats.GetSkill(i);
        var card = pool[i];
        card.index = i;
        card.gameObject.SetActive(true);

        if (card.icon && s != null && s.icon != null)
        card.icon.sprite = s.icon;
        if (card.nameText)  card.nameText.text = s.displayName;

        int cost = s.CostForNextLevel();
        bool isMax = (cost < 0);

        if (card.actionLabel)  card.actionLabel.text = isMax ? "Maxed" : $"$ {cost}";
        if (card.actionButton) card.actionButton.interactable =
            !isMax && wallet != null && wallet.amount >= cost;
    }
}
