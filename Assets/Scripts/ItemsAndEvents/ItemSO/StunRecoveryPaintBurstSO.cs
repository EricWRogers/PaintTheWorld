using UnityEngine;

[CreateAssetMenu(fileName = "StunRecoveryPaintBurst", menuName = "Items/Rare/Stun Recovery Paint Burst")]
public class StunRecoveryPaintBurstSO : ItemSO
{
    [Header("Ground Splash")]
    public float basePaintRadius = 2.0f;
    public float radiusPerStack = 0.75f;
    public float hardness = 1f;
    public float strength = 1f;

    [Header("Glob Ring")]
    public int baseGlobCount = 8;
    public int globCountPerStack = 2;
    public float spawnHeight = 0.5f;
    public float globRingRadius = 1.2f;
    public float outwardForceMultiplier = 1f;

    public override void OnEnemyRecoveredFromStun(PlayerContext ctx, GameObject enemy, int count)
    {
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
