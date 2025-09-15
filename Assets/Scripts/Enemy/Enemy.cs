using SuperPupSystems.Helper;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject player;

    [Header("Stats")]
    public int damage = 20;
    public int health = 100;
    public float attackRange;

    [Header("Animation")]
    public Animator anim;

    [Header("Attack Zones")]
    public Vector3 hitboxOffset;
    public Vector3 hitboxSize;
    public LayerMask layerMask;
    public bool hitboxActive = false;

    private bool m_hitPlayer;
    void Awake()
    {
        GetComponent<Health>().maxHealth = health;
    }
    void Start()
    {

    }

    void Update()
    {
        if (hitboxActive)
        {
            if (Physics.BoxCast(transform.position + hitboxOffset, hitboxSize, transform.forward, out RaycastHit hitInfo, transform.rotation, layerMask))
            {
                if (!m_hitPlayer)
                {
                    DamagePlayer();
                }
            }
        }
    }

    public void DamagePlayer()
    {
        Debug.Log("HitPlayer");
        player.GetComponent<Health>().Damage(damage);
        m_hitPlayer = true;
    }

    public void TurnOnHitBox()
    {
        Debug.Log("hitbox active is " + hitboxActive);
        if (!hitboxActive)
        {
            hitboxActive = true;
        }
    }
    public void ResetHitBox()
    {
        Debug.Log("hitbox active is " + hitboxActive);
        if (hitboxActive)
        {
            hitboxActive = false;
            m_hitPlayer = false;
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + hitboxOffset, hitboxSize);
    }
}
