using SuperPupSystems.Helper;
using UnityEngine;
using System.Collections.Generic;

// Handles a continuous particle stream weapon for painting and damage.
public class PaintBrush : Weapon
{
    [Header("Particle Beam")]
    public ParticleSystem beamParticles; 			 // The Particle System component to control
    public LayerMask particleHitLayers; 			     // Layers the beam should affect (Enemies, World, Paintable, etc.)
    [Range(0f, 10f)]
    public float damagePerSecond = 10f; 			 // Base DPS applied while particle collides with an enemy
    
    // No aiming variables needed; rotation is handled by the parent object (Camera/Player)

    private ParticleSystem.CollisionModule collisionModule;
    private readonly List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();
    private readonly Dictionary<GameObject, float> damageAccumulators = new Dictionary<GameObject, float>();
    
    private Color currentPaintColor = Color.white;
    private int currentDamage = 1;
    private bool wasFirePressedLastFrame = false;

    // --- Animation variables removed ---

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip fireSound; // Used for the continuous loop sound
    [Range(0f, 1f)]
    public float fireSoundVolume = 0.7f;

    void Awake()
    {
        if (audioSource == null)
        {
            // Set up a new AudioSource component if one isn't assigned
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f; // 3D sound
            audioSource.loop = true; // Set to loop for continuous beam sound
        }
    }

    new void Start()
    {
        base.Start();

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
        
        // --- Audio Control ---
        if (isFiring && !wasFirePressedLastFrame)
        {
            if (fireSound != null && audioSource != null)
            {
                audioSource.clip = fireSound;
                audioSource.volume = fireSoundVolume;
                audioSource.Play();
            }
        }
        else if (!isFiring && wasFirePressedLastFrame)
        {
            if (audioSource != null) audioSource.Stop();
            damageAccumulators.Clear(); // Clear accumulated fractional damage when beam stops
        }

        // --- Firing State Update (Calls Fire() continuously) ---
        if (isFiring)
        {
            Fire();
        }

        wasFirePressedLastFrame = isFiring;
    }

    public override void Fire()
    {
        // Used to refresh current weapon color/damage stats every frame while firing.
        
        // Get current paint color and damage from player components
        var playerPaint = player.GetComponent<PlayerPaint>();
        var playerWeapon = player.GetComponent<PlayerWeapon>();

        if (playerPaint != null)
        {
            currentPaintColor = playerPaint.selectedPaint;
        }

        // Use configured DPS or weapon damage as fallback for particle collision damage
        currentDamage = playerWeapon != null ? playerWeapon.damage : Mathf.RoundToInt(damagePerSecond);
    }
    
    // --- Removed animation-related methods: ResetAttack, etc. ---

    // New method to handle continuous painting and damage on particle collision
    void OnParticleCollision(GameObject other)
    {
        if (beamParticles == null) return;

        int num = beamParticles.GetCollisionEvents(other, collisionEvents);
        if (num == 0) return;

        // Process collision events
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
                // Paint the surface at the collision intersection point
                PaintManager.instance.paint(paintable, ev.intersection, player?.GetComponent<PlayerWeapon>()?.paintRadius ?? 1f, 1f, 1f, currentPaintColor);
            }

            // --- Damage Logic (as DPS) ---
            if (hitObj.CompareTag("Enemy"))
            {
                float skillMult = PlayerManager.instance.stats.skills[1].currentMult;
                // Choose DPS source
                float dps = damagePerSecond > 0f ? damagePerSecond : currentDamage;
                float add = dps * skillMult * Time.deltaTime;

                // Accumulate fractional damage per target
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