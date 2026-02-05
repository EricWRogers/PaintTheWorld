using UnityEngine;

public static class PlayerInputLock
{
    public static bool Locked { get; private set; }
    public static void SetLocked(bool v) => Locked = v;
}
