using UnityEngine;
using SuperPupSystems.Helper;
using KinematicCharacterControler;

public class Mortar : Enemy
{
    [Header("Mortar Settings")]
    public float aimDelay = 1.5f;
    public float flightTime = 2f;
    public float fireCooldown = 4f;
    public float extraHeight;
    public LayerMask groundMask;
    public LayerMask wallMask;

    [Header("Indicator Settings")]
    public GameObject targetIndicatorPrefab;

    private float m_attackTimer = 0f;
    private Vector3 m_targetPos;
    private GameObject m_currentIndicator;
    private bool m_hasTarget;


    void Update()
    {
        if (player == null) return;

        m_attackTimer -= Time.deltaTime;

        if (m_attackTimer <= 0)
        {
            Attack();
            m_attackTimer = fireCooldown;
        }
    }

    public override void Attack()
    {
        if (!m_hasTarget)
        {
            m_targetPos = GetPlayerTargetPos();
            m_hasTarget = true;

            ShowTargetIndicator(m_targetPos);
            Invoke(nameof(FireShell), aimDelay);
        }
    }

    private void FireShell()
    {
        if (bulletPrefab == null || firePoint == null) return;

        GameObject shell = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        shell.GetComponent<MortarShell>().Launch(firePoint.position, m_targetPos, flightTime, extraHeight);

        HideTargetIndicator();
        m_hasTarget = false;
    }

    private Vector3 GetPlayerTargetPos()
    {
        PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();

        RaycastHit hit;

        if (playerMovement.leftWall)
        {
            if (Physics.Raycast(player.transform.position, -player.transform.right, out hit, 5f, wallMask))
                return hit.point;
        }
        else if (playerMovement.rightWall)
        {
            if (Physics.Raycast(player.transform.position, player.transform.right, out hit, 5f, wallMask))
                return hit.point;
        }

        if (Physics.Raycast(player.transform.position + Vector3.up, Vector3.down, out hit, 100f, groundMask))
            return hit.point;

        return player.transform.position;
    }

    
    private void ShowTargetIndicator(Vector3 position)
    {
        if (targetIndicatorPrefab == null) return;

        if (m_currentIndicator != null)
            Destroy(m_currentIndicator);

        m_currentIndicator = Instantiate(targetIndicatorPrefab, position, Quaternion.identity, transform);
    }

    private void HideTargetIndicator()
    {
        if (m_currentIndicator != null)
            Destroy(m_currentIndicator);
    }
}
