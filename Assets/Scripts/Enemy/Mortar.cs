using UnityEngine;
using SuperPupSystems.Helper;
using KinematicCharacterControler;
using DG.Tweening;

public class Mortar : Enemy
{
    [Header("Mortar Settings")]
    public float aimDelay = 1.5f;
    public float flightTime = 2f;
    public float fireCooldown = 4f;
    public float extraHeight;
    public LayerMask groundMask;
    public LayerMask wallMask;
    public float hitRadius;

    [Header("Indicator Settings")]
    public GameObject targetIndicatorPrefab;

    private float m_attackTimer = 0f;
    private Vector3 m_targetPos;
    private GameObject m_currentIndicator;
    [HideInInspector] public bool hasTarget;


    void Update()
    {
        if (player == null) return;

        m_attackTimer -= Time.deltaTime;

        if (m_attackTimer <= 0)
        {
            Attack();
            m_attackTimer = fireCooldown;
        }
        if(!hasTarget)
        {
            Vector3 direction = PlayerManager.instance.player.transform.position - transform.position;
            direction.y = 0;

            if (direction == Vector3.zero)
                return;

            direction = Vector3.Normalize(direction);

            float angle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.localEulerAngles = new Vector3(0, angle, 0);
            
        }
    }

    public override void Attack()
    {
        if (!hasTarget)
        {
            m_targetPos = GetPlayerTargetPos();
            hasTarget = true;

            ShowTargetIndicator(m_targetPos);
            Invoke(nameof(FireShell), aimDelay);
        }
    }

    private void FireShell()
    {
        if (bulletPrefab == null || firePoint == null) return;

        MortarShell shell = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity).GetComponent<MortarShell>();
        shell.mortar = this;
        shell.Launch(firePoint.position, m_targetPos, flightTime, extraHeight, hitRadius);
        
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
        m_currentIndicator = Instantiate(targetIndicatorPrefab, position, Quaternion.identity, transform);
        m_currentIndicator.transform.localScale = Vector3.zero;
        m_currentIndicator.transform.DOScale(new Vector3(hitRadius, hitRadius, hitRadius), aimDelay);
        Destroy(m_currentIndicator, aimDelay + flightTime);
    }
}
