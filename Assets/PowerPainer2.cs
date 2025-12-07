using UnityEngine;
using SuperPupSystems.Helper;
using System.Collections.Generic;

// LineRenderer and Raycast aiming logic removed.
public class PowerPainter2 : Weapon
{
    [Header("Particle Beam")]
    public ParticleSystem beamParticles; 			 // assign your particle system here
    public LayerMask particleHitLayers; 			     // layers the beam should affect (Enemies, World, Paintable, etc.)
    [Range(0f, 10f)]
    public float damagePerSecond = 10f; 			 // base DPS applied while particle collides with an enemy
    
    // No aiming variables needed; rotation is handled by the parent object (Camera/Player)

    private ParticleSystem.CollisionModule collisionModule;
    private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private Color currentPaintColor = Color.white;
    private int currentDamage = 1;

    // per-target fractional accumulator so we apply integer damage deterministically
    private readonly Dictionary<GameObject, float> damageAccumulators = new Dictionary<GameObject, float>();

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip loopFireSound;
    [Range(0f, 1f)]
    public float fireSoundVolume = 0.7f;

    private bool wasFirePressedLastFrame = false;
    private GameObject m_player;

    new void Start()
    {
        base.Start();
        m_player = player;

        if (audioSource == null)
        {
            // Set up a new AudioSource component if one isn't assigned
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.loop = true;
        }

        if (beamParticles != null)
        {
            // Configure the Particle System's collision module for interaction
            collisionModule = beamParticles.collision;
            collisionModule.enabled = true;
            collisionModule.sendCollisionMessages = true;
            collisionModule.collidesWith = particleHitLayers;
        }
    }

    new void Update()
    {
        base.Update();

        bool isFiring = playerInputs.Attack.IsPressed();
        
        // --- Audio Start/Stop Logic ---
        if (isFiring && !wasFirePressedLastFrame)
        {
            if (loopFireSound != null && audioSource != null)
            {
                audioSource.clip = loopFireSound;
                audioSource.volume = fireSoundVolume;
                audioSource.Play();
            }
        }
        else if (!isFiring && wasFirePressedLastFrame)
        {
            if (audioSource != null) audioSource.Stop();
            damageAccumulators.Clear(); // Clear accumulated fractional damage when beam stops
        }

        // --- Particle System Play/Stop Logic ---
        if (beamParticles != null)
        {
            if (isFiring)
            {
                if (!beamParticles.isPlaying) beamParticles.Play();
            }
            else
            {
                if (beamParticles.isPlaying) beamParticles.Stop();
            }
        }

        // Prepare current color/damage each frame while firing
        if (isFiring)
        {
            Fire();
        }

        wasFirePressedLastFrame = isFiring;
    }

    // Aiming function removed. The weapon GameObject should be a child of the camera.
    // The rotation of the camera dictates the direction of the particle emission.

    public override void Fire()
    {
        if (m_player == null) return;

        var playerPaint = m_player.GetComponent<PlayerPaint>();
        var playerWeapon = m_player.GetComponent<PlayerWeapon>();

        currentPaintColor = playerPaint.selectedPaint;
        // use configured DPS or weapon damage as fallback
        currentDamage = playerWeapon != null ? playerWeapon.damage : Mathf.RoundToInt(damagePerSecond);
    }

    // Called by ParticleSystem when particles hit colliders 
    void OnParticleCollision(GameObject other)
    {
        if (beamParticles == null) return;

        int num = beamParticles.GetCollisionEvents(other, collisionEvents);
        if (num == 0) return;

        // Process events
        for (int i = 0; i < num; i++)
        {
            var ev = collisionEvents[i];
            GameObject hitObj = ev.colliderComponent != null ? ev.colliderComponent.gameObject : other;

            // layer filter
            if (((1 << hitObj.layer) & particleHitLayers.value) == 0) continue;

            // --- Painting Logic ---
            var paintable = hitObj.GetComponent<Paintable>();
            if (paintable != null)
            {
                // paint at collision intersection point
                PaintManager.instance.paint(paintable, ev.intersection, player?.GetComponent<PlayerWeapon>()?.paintRadius ?? 1f, 1f, 1f, currentPaintColor);
            }

            // --- Damage Logic (as DPS) ---
            if (hitObj.CompareTag("Enemy"))
            {
                float skillMult = PlayerManager.instance.stats.skills[1].currentMult;
                float dps = damagePerSecond > 0f ? damagePerSecond : currentDamage;
                float add = dps * skillMult * Time.deltaTime;

                damageAccumulators.TryGetValue(hitObj, out float acc);
                acc += add;

                if (acc >= 1f)
                {
                    int apply = Mathf.FloorToInt(acc);
                    var h = hitObj.GetComponent<Health>();
                    if (h != null) h.Damage(apply);
                    acc -= apply;
                }

                damageAccumulators[hitObj] = acc;
            }
        }
    }

    public void DestroyGun()
    {
        if (beamParticles != null) beamParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        if (audioSource != null) audioSource.Stop();
        damageAccumulators.Clear();
        Destroy(gameObject);
    }
}