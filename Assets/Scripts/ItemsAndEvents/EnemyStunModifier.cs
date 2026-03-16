using UnityEngine;

public class EnemyStunModifier : MonoBehaviour
{
    public static float extraStunTime = 0f;
    public static float extraRecoveryGrace = 0f;

    public static void Configure(float stunBonus, float graceBonus)
    {
        extraStunTime = stunBonus;
        extraRecoveryGrace = graceBonus;
    }

    public static void ResetModifiers()
    {
        extraStunTime = 0f;
        extraRecoveryGrace = 0f;
    }
}