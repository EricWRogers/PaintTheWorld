
using System;
using UnityEngine;

public enum HitSource
{
    PlayerWeapon,      // normal player hits 
    PaintLauncherGlob, // Must not cause more globs
    GiftOfLifeGlob     // counts as normal hit but can trigger paint launcher
}

public static class GameEvents
{
    public static Action<GameObject,int,HitSource> PlayerHitEnemy; // (enemy, dmg, source)
    public static Action<int> PlayerDamaged;                        // damage to player
    public static Action<int> PlayerHealed;                         // heal player
    public static Action<GameObject> EnemyKilled;                   // enemy gameobjectt

    public static event Action WallRunStarted;
    public static event Action WallRunTick;

    public static System.Action PlayerDodged;                     // fires when a dash begins
    public static System.Action PlayerStartedGrinding;            // fires when rail grind starts
    public static System.Action PlayerGrindingTick;               // fires periodically while grinding

    // Paint progress, call when we use paint
    public static System.Action<float> PaintApplied;              // amount added, add units later

    public static void RaiseWallRunStarted() => WallRunStarted?.Invoke();
    public static void RaiseWallRunTick()    => WallRunTick?.Invoke();
}
