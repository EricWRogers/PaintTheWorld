using UnityEngine;
using SuperPupSystems.Helper;

public class DebugHealthInput : MonoBehaviour
{
    public Health health;

    private void Awake()
    {
        if (!health) health = GetComponent<Health>();
    }

    private void Update()
    {
        if (!health) return;

        if (Input.GetKeyDown(KeyCode.H)) health.Damage(10);  // Hurt 10
        if (Input.GetKeyDown(KeyCode.J)) health.Heal(10);    // Heal 10
        if (Input.GetKeyDown(KeyCode.K)) health.Revive();    // Revive to max
        if (Input.GetKeyDown(KeyCode.L)) health.Kill();      // Set to 0
    }
}
