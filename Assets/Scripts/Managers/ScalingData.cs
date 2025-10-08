using UnityEngine;

[CreateAssetMenu(fileName = "EnemyScalingData", menuName = "Game/Enemy Scaling Data")]
public class EnemyScalingData : ScriptableObject
{
    [Header("Scaling Amounts Per Stage")]
    [Tooltip("How much extra damage enemies gain per stage (as a percent of base damage). Ex: 0.1 = +10% per stage")]
    public float enemyDamageIncrease = 0.1f;

    [Tooltip("How much extra boss health per stage (as a percent of base health). Ex: 0.2 = +20% per stage")]
    public float bossHealthIncrease = 0.2f;

    [Tooltip("How much extra enemies to spawn per stage (as a percent). Ex: 0.05 = +5% per stage")]
    public float enemyCountIncrease = 0.05f;
    [Tooltip("How much extra enemies to spawn per stage (as a percent). Ex: 0.05 = +5% per stage")]
    public float coinGainIncrease = 0.05f;

    [Header("Loop Bonuses (applied each loop)")]
    public float loopDamageBonus = 0.25f;
    public float loopBossBonus = 0.3f;
    public float loopEnemyCountBonus = 0.1f;
    public float loopCoinGainBonus;
}