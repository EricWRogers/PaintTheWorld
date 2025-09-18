using SuperPupSystems.Helper;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

public class Boss : MonoBehaviour
{
    public GameObject player;

    [Header("Stats")]
    public int health = 100;
    public float speed = 4;
    public float rotationSpeed = 10;
    public float attackRange;

    [Header("Animation")]
    //public Animator anim;
    //public bool inAttackAnim = false;

    [Header("Attack Details")]
    public GameObject hitboxObj;
    public LayerMask layerMask;
    public bool hitboxActive = false;

    private BoxCollider m_hitbox;
    private bool m_hitPlayer;
    protected NavMeshAgent p_agent;
    protected Rigidbody p_rb;

    [Header("laser targeting")]
    public GameObject laserFirePoint;
    public float laserRotationSpeed = 360f;
    public bool yawOnly = true;
    [Header("laser Timers")]
    public float windupTime = 1.2f;
    public float fireTime = 2.0f;
    public float laserCooldown;
    public bool canLaser;

    [Header("laser stats")]
    public float maxBeamDistance = 50f;
    public float beamWidth = 0.12f;
    public int damagePerSecond = 30;
    [Header("laser Visuals")]
    public Gradient telegraphGradient;
    public Gradient firingGradient;
    public float telegraphWidth = 0.08f;

    public LineRenderer lr;

    [Header("Swing stats")]
    public int swingDamage;
    public bool canSwing;
    public float swingCooldown;

    [Header("Slam stats")]
    public int slamDamage;
    public bool canSlam;
    public float slamCooldown;

    private float m_speacialCur = 0;
    private float m_swingCur = 0;
    private float m_slamcur = 0;

    void Awake()
    {
        GetComponent<Health>().maxHealth = health;
        p_agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player");
        m_hitbox = hitboxObj.GetComponent<BoxCollider>();
        p_rb = GetComponent<Rigidbody>();
        p_agent.speed = speed;

        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.widthCurve = AnimationCurve.Constant(0, 1, beamWidth);
        lr.enabled = false;

    }
    void OnValidate()
    {
        if (lr != null)
        {
            lr.widthCurve = AnimationCurve.Constant(0, 1, beamWidth);
        }
    }
    void Start()
    {
        m_slamcur = -Time.deltaTime;
        m_speacialCur = -Time.deltaTime;
        m_swingCur = -Time.deltaTime;

        if (m_slamcur <= 0)
        {
            canSlam = true;
        }
        else canSlam = false;

        if (m_speacialCur <= 0)
        {
            canLaser = true;
        }
        else canLaser = false;

        if (m_swingCur <= 0)
        {
            canSwing = true;
        }
        else canSwing = false;

    }

    void Update()
    {

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
        /*if (inAttackAnim)
        {
            inAttackAnim = false;
        }
        else
        {
            inAttackAnim = true;
        }*/
    }
    
}
