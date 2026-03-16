using UnityEngine;
using KinematicCharacterControler;

/// <summary>
/// Place this on a ramp trigger volume.
/// When the player walks through it, a strong upward force is added to the player's velocity
/// so they launch cleanly off the ramp.
/// </summary>
[RequireComponent(typeof(Collider))]
public class BoostRamp : MonoBehaviour
{
    [Header("Boost Settings")]
    [Tooltip("Base upward velocity added when the player hits the ramp.")]
    public float boostForce = 18f;

    [Tooltip("Multiplies the player's current horizontal speed and adds it as extra upward force. " +
             "Rewards going fast — set to 0 to disable.")]
    public float speedToUpwardBonus = 0.25f;

    [Tooltip("Hard cap on the total upward velocity applied (prevents one-shotting very high speeds).")]
    public float maxBoostForce = 30f;

    [Tooltip("Direction the boost launches the player. Defaults to the ramp's local Up if left at zero. " +
             "Set this to angle the launch (e.g. slightly forward off a ski jump).")]
    public Vector3 boostDirection = Vector3.zero;

    [Tooltip("How long to ignore re-triggering after the first boost. Stops double-firing on slopes.")]
    public float reTriggerCooldown = 0.4f;

    [Header("Feedback")]
    public ParticleSystem boostParticles;
    public AudioClip boostSound;

    // ------------------------------------------------------------------ //

    private float m_lastBoostTime = -999f;
    private AudioSource m_audio;

    void Awake()
    {
        // Make sure the collider is a trigger
        GetComponent<Collider>().isTrigger = true;

        if (boostSound != null)
        {
            m_audio = gameObject.AddComponent<AudioSource>();
            m_audio.playOnAwake = false;
            m_audio.clip = boostSound;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Cooldown guard
        if (Time.time - m_lastBoostTime < reTriggerCooldown) return;

        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player == null) return;

        ApplyBoost(player);
        m_lastBoostTime = Time.time;
    }

    void ApplyBoost(PlayerMovement player)
    {
        // Decide launch direction
        Vector3 launchDir = (boostDirection.sqrMagnitude > 0.001f)
            ? boostDirection.normalized
            : Vector3.up;

        // Horizontal speed bonus: faster approach = bigger pop
        float horizontalSpeed = new Vector3(player.Velocity.x, 0f, player.Velocity.z).magnitude;
        float totalBoost = boostForce + horizontalSpeed * speedToUpwardBonus;
        totalBoost = Mathf.Clamp(totalBoost, 0f, maxBoostForce);

        // Apply the boost by adding to the velocity
        Vector3 boostVector = launchDir * totalBoost;
        player.AddForce(boostVector);
        Debug.DrawRay(player.transform.position, boostVector, Color.green, 2f);
        Debug.DrawRay(player.transform.position, launchDir, Color.red, 2f);
        Debug.DrawRay(player.transform.position, transform.up, Color.blue, 2f);
        Debug.Log($"Applying boost! Base: {boostForce:F1}, Speed Bonus: {horizontalSpeed * speedToUpwardBonus:F1}, Total: {totalBoost:F1}");
     
    }


    void OnDrawGizmos()
    {
        // Show the launch direction in the editor so you can eyeball the arc
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.85f);
        Vector3 dir = (boostDirection.sqrMagnitude > 0.001f)
            ? boostDirection.normalized
            : transform.up;

        Gizmos.DrawRay(transform.position, dir * 3f);
        Gizmos.DrawWireSphere(transform.position, 0.2f);

        // Show trigger bounds
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.25f);
        Collider col = GetComponent<Collider>();
        if (col != null)
            Gizmos.DrawCube(col.bounds.center, col.bounds.size);

        }
        
}