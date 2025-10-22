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

    new void Start()
    {
        base.Start(); // call Weapon.Start()

        m_player = player;
        m_lineRender = GetComponent<LineRenderer>();
        painter = new RayCastPainter();

        m_lineRender.enabled = false;
    }

    new void Update()
    {
        base.Update();
        // Just handle player input here
        if (playerInputs.Attack.IsPressed())
        {
            Fire();
        }
        else
        {
            m_lineRender.enabled = false;
        }
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
    }
}
