using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class SkillData
{
    public string id = "Health";            // unique key for health
    public string displayName = "Health";
    [TextArea] public string description;
    public Sprite icon;

    [Header("Progress")]
    public int level = 0;
    public int maxLevel = 5;
    public float levelGrowth = .5f;
    public float currentMult = 1;

    [Header("Cost Curve")]
    public int baseCost = 50;               // cost at level 0 to 1
    public float costGrowth = 1.5f;         // multiplicated growth per level

    public int CostForNextLevel()
    {
        if (level >= maxLevel) return -1;
        // ceil( baseCost * growth^level )
        return Mathf.CeilToInt(baseCost * Mathf.Pow(costGrowth, level));
    }
}

[Serializable] public class SkillChangedEvent : UnityEvent<int, SkillData> {}

public class PlayerStats : MonoBehaviour
{
    [Header("Skills")]
    public List<SkillData> skills = new List<SkillData>()
    {
        new SkillData{ id="Health", displayName="Health", description="Max HP up.", baseCost=50, costGrowth=1.5f, levelGrowth = .5f, maxLevel=5 },
        new SkillData{ id="Damage", displayName="Damage", description="Attack power up.", baseCost=60, costGrowth=1.5f, levelGrowth = .5f, maxLevel=5 },
        new SkillData{ id="Radius", displayName="Radius", description="Increase paint raddius", baseCost=55, costGrowth=1.5f, levelGrowth = .5f, maxLevel=5 },
        new SkillData{ id="Move Speed", displayName="Move Speed", description="Move faster.", baseCost=45, costGrowth=1.6f, levelGrowth = .5f, maxLevel=6 },
        new SkillData{ id="Attack Speed", displayName="Attack Speed", description="Attack faster.", baseCost=45, costGrowth=1.6f, levelGrowth = .5f, maxLevel=6 },
    };

    [Header("Events")]
    public SkillChangedEvent onSkillChanged;

    private void Awake()
    {
        if (onSkillChanged == null) onSkillChanged = new SkillChangedEvent();
    }

    public SkillData GetSkill(int index) => (index >= 0 && index < skills.Count) ? skills[index] : null;

    /// <summ>Attempts to upgrade a skill by spending from the wallet
    public bool TryUpgradeSkill(int index, Currency wallet)
    {
        var s = GetSkill(index);
        if (s == null || wallet == null) return false;
        if (s.level >= s.maxLevel) return false;

        int cost = s.CostForNextLevel();
        if (cost < 0 || wallet.amount < cost) return false;

        if (!wallet.Spend(cost)) return false;

        s.currentMult += s.levelGrowth;
        s.level++;
        onSkillChanged.Invoke(index, s);
        return true;
    }
}
