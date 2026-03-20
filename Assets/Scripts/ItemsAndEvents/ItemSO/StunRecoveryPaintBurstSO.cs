using UnityEngine;

[CreateAssetMenu(fileName = "StunRecoveryPaintBurst", menuName = "Items/Rare/Stun Recovery Paint Burst")]
public class StunRecoveryPaintBurstSO : ItemSO
{
    [Header("Ground Splash")]
    public float basePaintRadius = 2.2f;
    public float radiusPerStack = 0.8f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Glob Ring")]
    public int baseGlobCount = 8;
    public int globCountPerStack = 2;
    public float spawnHeight = 1.5f;
    public float globRingRadius = 1.4f;
    public float outwardForceMultiplier = 0.02f;

    public override void OnEnemyRecoveredFromStun(PlayerContext ctx, GameObject enemy, int count)
    {
        Debug.Log($"[StunRecoveryPaintBurstSO] Triggered on {enemy.name} | stacks={count}");

        EnemyRecoverySplashEmitter.Emit(
            ctx,
            enemy,
            basePaintRadius,
            radiusPerStack,
            baseGlobCount,
            globCountPerStack,
            hardness,
            strength,
            spawnHeight,
            globRingRadius,
            outwardForceMultiplier,
            count
        );
    }
}