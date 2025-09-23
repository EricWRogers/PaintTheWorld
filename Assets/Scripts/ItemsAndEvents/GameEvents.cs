
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
}
