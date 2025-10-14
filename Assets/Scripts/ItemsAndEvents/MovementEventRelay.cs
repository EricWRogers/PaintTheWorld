using UnityEngine;

[RequireComponent(typeof(PlayerMovement))]
public class MovementEventRelay : MonoBehaviour
{
    public float grindTickInterval = 0.6f;

    private PlayerMovement pm;
    private bool wasDashing;
    private bool wasGrinding;
    private float grindTimer;

    void Awake() { pm = GetComponent<PlayerMovement>(); }

    void Update()
    {
        // Dodge start detection , start of dash
        if (!wasDashing && pm.isDashing)
        {
            GameEvents.PlayerDodged?.Invoke();
        }
        wasDashing = pm.isDashing;

        // Grind start and periodic tics 
        if (!wasGrinding && pm.isGrinding)
        {
            GameEvents.PlayerStartedGrinding?.Invoke();
            grindTimer = 0f;
        }
        if (pm.isGrinding)
        {
            grindTimer += Time.deltaTime;
            if (grindTimer >= grindTickInterval)
            {
                grindTimer = 0f;
                GameEvents.PlayerGrindingTick?.Invoke();
            }
        }
        wasGrinding = pm.isGrinding;
    }
}
