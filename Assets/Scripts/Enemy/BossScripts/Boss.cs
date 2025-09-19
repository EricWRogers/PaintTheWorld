using SuperPupSystems.Helper;
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
    public bool isLasering;
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
    public GameObject objectToSpawn;
    public Transform centerPoint;
    public float jumpSpeed;
    public float slamSpeed;
    public int slamDamage;
    public bool canSlam;
    public float slamCooldown;
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 50f);
    public int totalSpawned = 0;
    public int poolSize = 20;
    public bool slamIsDone;
    private float m_swingCur = 0;
    private float m_slamcur = 0;

    private enum LaserPhase { Idle, Windup, Firing, Cooldown }
    private LaserPhase laserPhase = LaserPhase.Idle;
    private float laserTimer = 0f;
    private float damageAccumulator = 0f;
    private bool m_goingUp;
    private bool m_goingDown;

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
        m_goingUp = true;
        m_goingDown = false;

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
        m_slamcur -= Time.deltaTime;
        m_swingCur -= Time.deltaTime;

        if (m_slamcur <= 0)
        {
            canSlam = true;
        }

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
        if (inAttackAnim)
        {
            inAttackAnim = false;
        }
        else
        {
            inAttackAnim = true;
        }
    }
    public void GoToCenter()
    {
        transform.position = new Vector3(centerPoint.position.x, transform.position.y, centerPoint.position.z);
    }
    void SpawnObject()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(-spawnAreaSize.x / 2, spawnAreaSize.x / 2),
            Random.Range(-spawnAreaSize.y / 2, spawnAreaSize.y / 2),
            Random.Range(-spawnAreaSize.z / 2, spawnAreaSize.z / 2)
        ) + centerPoint.position;

        GameObject.Instantiate(objectToSpawn, spawnPosition, transform.rotation);
    }
    void OnDrawGizmos()
    {

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(centerPoint.position, spawnAreaSize);
    }
    public void SpawnAllObjects()
    {
        for (int i = 0; i < poolSize; i++)
        {
            SpawnObject();
            totalSpawned++;
        }
    }

    public void SlamAttack()
    {
        slamIsDone = false;
        m_slamcur = slamCooldown;
        if (transform.position.y >= 200f)
        {
            m_goingUp = false;
            m_goingDown = true;
        }
        if (m_goingUp)
        {
            transform.position += transform.up * jumpSpeed * Time.fixedDeltaTime;
        }
        else if (m_goingDown)
        {
            m_goingUp = false;
            transform.position = new Vector3(centerPoint.position.x, transform.position.y, centerPoint.position.z);
            transform.position += -transform.up * slamSpeed * Time.fixedDeltaTime;
        }
        if (Physics.Raycast(transform.position, -transform.up, out RaycastHit hit, 7f) && !m_goingUp)
        {
            m_goingDown = false;
            SpawnAllObjects();
            canSlam = false;
            m_goingUp = true;
            m_goingDown = false;
            slamIsDone = true;
        }
    }
    public void LaserBeam()
    {

        float delta = Time.deltaTime;
        switch (laserPhase)
        {
            case LaserPhase.Idle:
                lr.enabled = true;
                lr.widthCurve = AnimationCurve.Constant(0, 1, telegraphWidth);
                laserTimer = windupTime;
                laserPhase = LaserPhase.Windup;
                break;

            case LaserPhase.Windup:
                isLasering = true;
                laserTimer -= delta;
                RotateTowardsTarget(delta);

                float alpha = Mathf.PingPong(Time.time * 2f, 1f);
                SetLineRendererForTelegraph(alpha);

                Vector3 startW = laserFirePoint.transform.position;
                Vector3 dirW = GetAimDirection();
                lr.SetPosition(0, startW);
                lr.SetPosition(1, startW + dirW * maxBeamDistance);

                if (laserTimer <= 0f)
                {
                    laserTimer = fireTime;
                    lr.widthCurve = AnimationCurve.Constant(0, 1, beamWidth);
                    SetLineRendererGradient(firingGradient);
                    laserPhase = LaserPhase.Firing;
                }
                break;

            case LaserPhase.Firing:
                laserTimer -= delta;
                RotateTowardsTarget(delta);

                Vector3 startF = laserFirePoint.transform.position;
                Vector3 dirF = GetAimDirection();

                if (Physics.Raycast(startF, dirF, out RaycastHit hit, maxBeamDistance, layerMask))
                {

                    lr.SetPosition(0, startF);
                    lr.SetPosition(1, hit.point);

                    if (hit.collider.CompareTag("Player"))
                    {
                        damageAccumulator += damagePerSecond * Time.deltaTime;
                        if (damageAccumulator >= 1f)
                        {
                            int applyDamage = Mathf.FloorToInt(damageAccumulator);
                            hit.collider.GetComponent<Health>().Damage(applyDamage);
                            damageAccumulator -= applyDamage;
                        }
                    }
                }
                else
                {
                    lr.SetPosition(0, startF);
                    lr.SetPosition(1, startF + dirF * maxBeamDistance);
                }

                if (laserTimer <= 0f)
                {
                    lr.enabled = false;
                    isLasering = false;
                    laserTimer = laserCooldown;
                    laserPhase = LaserPhase.Cooldown;
                }
                break;

            case LaserPhase.Cooldown:
                laserTimer -= delta;
                if (laserTimer <= 0f)
                {
                    laserPhase = LaserPhase.Idle;
                }
                break;
        }
    }

    private Vector3 GetAimDirection()
    {
        return laserFirePoint.transform.forward;
    }

    private void RotateTowardsTarget(float delta)
    {
        if (player == null) return;
        Vector3 toTarget = player.transform.position - laserFirePoint.transform.position;
        if (yawOnly) toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        laserFirePoint.transform.rotation = Quaternion.RotateTowards(laserFirePoint.transform.rotation, targetRot, laserRotationSpeed * delta);
    }

    void SetLineRendererForTelegraph(float alpha)
    {
        Gradient g = new Gradient();
        GradientColorKey[] colorKeys = telegraphGradient.colorKeys;
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colorKeys.Length];
        for (int i = 0; i < colorKeys.Length; i++)
        {
            alphaKeys[i].alpha = telegraphGradient.Evaluate(colorKeys[i].time).a * alpha;
            alphaKeys[i].time = colorKeys[i].time;
        }
        g.SetKeys(colorKeys, alphaKeys);
        lr.colorGradient = g;
    }

    void SetLineRendererGradient(Gradient source)
    {
        Gradient g = new Gradient();
        g.SetKeys(source.colorKeys, source.alphaKeys);
        lr.colorGradient = g;
    }

    public void SwingAttack()
    {
        
    }
}
