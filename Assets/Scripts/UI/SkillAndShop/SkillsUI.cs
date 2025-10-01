using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SkillsUI : MonoBehaviour
{
    [Header("Refs")]
    public PlayerStats stats;
    public Currency wallet;

    [System.Serializable]
    public class Row
    {
        public Image icon;
        public TMP_Text nameText;
        public TMP_Text descText;
        public TMP_Text levelText;
        public TMP_Text costText;
        public Button upgradeButton;
    }

    [Header("UI Rows (4)")]
    public Row[] rows = new Row[4];

    private void Awake()
    {
        if (!stats) stats = PlayerManager.instance.stats;
        if (!wallet) wallet = PlayerManager.instance.wallet;
    }

    private void OnEnable()
    {
        if (stats) stats.onSkillChanged.AddListener(OnSkillChanged);
        if (wallet && wallet.changed != null) wallet.changed.AddListener(OnWalletChanged);
        RefreshAll();
    }

    private void OnDisable()
    {
        if (stats) stats.onSkillChanged.RemoveListener(OnSkillChanged);
        if (wallet && wallet.changed != null) wallet.changed.RemoveListener(OnWalletChanged);
    }

    public void BindButtons()
    {
        for (int i = 0; i < rows.Length; i++)
        {
            int idx = i;
            if (rows[i]?.upgradeButton != null)
            {
                rows[i].upgradeButton.onClick.RemoveAllListeners();
                rows[i].upgradeButton.onClick.AddListener(() => TryUpgrade(idx));
            }
        }
    }

    private void Start()
    {
        BindButtons();
        RefreshAll();
    }

    private void TryUpgrade(int index)
    {
        if (!stats || !wallet) return;
        stats.TryUpgradeSkill(index, wallet);
        RefreshRow(index);
    }

    private void OnSkillChanged(int index, SkillData data) => RefreshRow(index);
    private void OnWalletChanged(int amt) => RefreshAll();

    private void RefreshAll()
    {
        for (int i = 0; i < rows.Length; i++)
            RefreshRow(i);
    }

    private void RefreshRow(int i)
    {
        if (stats == null || i < 0 || i >= rows.Length) return;
        var row = rows[i];
        var s = stats.GetSkill(i);
        if (row == null || s == null) return;

        if (row.icon) row.icon.sprite = s.icon;
        if (row.nameText) row.nameText.text = s.displayName;
        if (row.descText) row.descText.text = s.description;
        if (row.levelText) row.levelText.text = $"Lv {s.level}/{s.maxLevel}";

        int cost = s.CostForNextLevel();
        bool isMax = (cost < 0);
        if (row.costText) row.costText.text = isMax ? "Maxed" : $"Cost: {cost}";
        if (row.upgradeButton)
        {
            bool canAfford = !isMax && wallet && wallet.amount >= cost;
            row.upgradeButton.interactable = canAfford;
        }
    }
}

