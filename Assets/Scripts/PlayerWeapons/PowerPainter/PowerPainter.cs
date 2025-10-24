using UnityEngine;
using SuperPupSystems.Helper;

[RequireComponent(typeof(LineRenderer))]
public class PowerPainter : Weapon
{
    [Header("Power Painter Settings")]
    public float rayDistance = 100f;
    public LayerMask ignoreLayers;
    public Transform rayStart;

    private RayCastPainter painter;
    private LineRenderer m_lineRender;
    private GameObject m_player;
    private float damageAccumulator = 0f;

    [Header("Audio")]
    public AudioSource audioSource;      
    public AudioClip loopFireSound; 
    [Range(0f, 1f)]
    public float fireSoundVolume = 0.7f;

    private bool wasFirePressedLastFrame = false;

    new void Start()
    {
        base.Start(); // call Weapon.Start()

        m_player = player;
        m_lineRender = GetComponent<LineRenderer>();
        painter = new RayCastPainter();

        m_lineRender.enabled = false;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1f;
            audioSource.loop = true;       // For continuous beam sound
        }
    }

    new void Update()
    {
        base.Update();

        bool isFiring = playerInputs.Attack.IsPressed();
        // Just handle player input here
         if (isFiring && !wasFirePressedLastFrame)
        {
            // Start firing
            if (loopFireSound != null)
            {
                audioSource.clip = loopFireSound;
                audioSource.volume = fireSoundVolume;
                audioSource.Play();
            }
        }
        else if (!isFiring && wasFirePressedLastFrame)
        {
            // Stop firing
            audioSource.Stop();
        }
        if (isFiring)
        {
            Fire();
        }
        else
        {
            m_lineRender.enabled = false;
        }
        wasFirePressedLastFrame = isFiring;
    }

    public override void Fire()
    {
        if (m_player == null) return;

        // Update color and damage based on player data
        var playerPaint = m_player.GetComponent<PlayerPaint>();
        var playerWeapon = m_player.GetComponent<PlayerWeapon>();

        Color paintColor = playerPaint.selectedPaint;
        int dmg = playerWeapon.damage;
        float radius = playerWeapon.paintRadius;

        m_lineRender.startColor = paintColor;
        m_lineRender.endColor = paintColor;
        m_lineRender.enabled = true;

        Ray ray = new Ray(rayStart.position, rayStart.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, rayDistance, ~ignoreLayers))
        {
            m_lineRender.SetPosition(0, rayStart.position);
            m_lineRender.SetPosition(1, hit.point);

            // Paintable surface
            if (hit.collider.TryGetComponent(out Paintable paintable))
            {
                painter.TryPaint(hit);
            }

            // Enemy damage
            if (hit.transform.CompareTag("Enemy"))
            {
                damageAccumulator += dmg * Time.deltaTime;
                if (damageAccumulator >= 1f)
                {
                    int applyDamage = Mathf.FloorToInt(damageAccumulator);
                    hit.transform.GetComponent<Health>().Damage(applyDamage);
                    //hit.transform.GetComponent<Enemy>().SpawnDamageText(applyDamage);
                    damageAccumulator -= applyDamage;
                }
            }
        }
        else
        {
            // No hit
            m_lineRender.SetPosition(0, rayStart.position);
            m_lineRender.SetPosition(1, rayStart.position + rayStart.forward * rayDistance);
        }
    }

    public void DestroyGun()
    {
        Destroy(gameObject);

        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
