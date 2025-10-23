using UnityEngine;
public class ItemEventProbe : MonoBehaviour
{
    void OnEnable()
    {
        GameEvents.PlayerHitEnemy += (e, d, s) => Debug.Log($"[HIT] {e?.name} {d} src={s}");
        GameEvents.PlayerDamaged += d => Debug.Log($"[PDAM] {d}");
        GameEvents.PlayerHealed += h => Debug.Log($"[PHEAL] {h}");
        GameEvents.EnemyKilled += e => Debug.Log($"[KILL] {e?.name}");
        GameEvents.PlayerDodged += () => Debug.Log("[DODGE]");
        GameEvents.PlayerStartedGrinding += () => Debug.Log("[GRIND START]");
        //GameEvents.PlayerGrindingTick += () => Debug.Log("[GRIND TICK]");
        //GameEvents.PaintApplied += a => Debug.Log($"[PAINT+] {a:0.00}");
    }
}

