using SuperPupSystems.Helper;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Enemy : MonoBehaviour
{
    public GameObject player;

    [Header("Stats")]
    public int damage = 20;
    public int health = 100;
    public float speed = 4;
    public float rotationSpeed = 10;
    public float attackRange;

    [Header("Animation")]
    public Animator anim;
    public bool inAttackAnim = false;

    [Header("Attack Details")]
    public GameObject hitboxObj;
    public LayerMask layerMask;
    public bool hitboxActive = false;

    private BoxCollider m_hitbox;
    private bool m_hitPlayer;
    protected NavMeshAgent p_agent;
    protected Rigidbody p_rb;
    public void Awake()
    {
        GetComponent<Health>().maxHealth = health;
        p_agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        m_hitbox = hitboxObj.GetComponent<BoxCollider>();
        p_rb = GetComponent<Rigidbody>();
        p_agent.speed = speed;
    }
    public void Start()
    {

    }

    public void Update()
    {
        if (hitboxActive)
        {
            if (Physics.BoxCast(hitboxObj.transform.position, m_hitbox.size / 2.0f, transform.forward, out RaycastHit hitInfo, transform.rotation, Vector3.Distance(hitboxObj.transform.localPosition, m_hitbox.center), layerMask))
            {
                Debug.Log(Vector3.Distance(hitboxObj.transform.localPosition, m_hitbox.center));
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
        if (!hitboxActive)
        {
            hitboxActive = true;
        }
    }
    public void ResetHitBox()
    {
        if (hitboxActive)
        {
            hitboxActive = false;
            m_hitPlayer = false;
        }
    }
    public void ToggleIsInAnim()
    {
        if (inAttackAnim)
        {
            inAttackAnim = false;
        }
        else
        {
            inAttackAnim = true;
        }
    }

    
}
