using SuperPupSystems.Helper;
using UnityEngine;
using UnityEngine.AI;

public class Boss : MonoBehaviour
{
    public GameObject m_player;

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
    public float scaledRotation;
    public bool yawOnly = true;
    public AnimationCurve accuracyCurve;
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

    public float swingDashSpeed;
    public float swingDashDistance;

    [Header("Slam stats")]
    public GameObject objectToSpawn;
    public Transform centerPoint;
    public float jumpSpeed;
    public float slamSpeed;
    public int slamDamage;
    public bool canSlam;
    public Vector3 slamOffset;
    public Vector3 spawnAreaSize = new Vector3(50f, 20f, 50f);
    public int totalSpawned = 0;
    public int poolSize = 20;
    public bool slamIsDone;

    [Header("Timers")]
    public float laserWindupTime = 1.2f;
    public float laserFireTime = 2.0f;
    public float laserCooldown;
    public float swingCooldown;
    public float slamCooldown;
    public float walkTime;
    public float m_swingCur = 0;
    public float m_slamcur = 0;
    public float m_laserCur = 0;

    [Header("On Death")]
    public Vector2 moneyToAdd;

    private enum LaserPhase { Idle, Windup, Firing }
    private LaserPhase laserPhase = LaserPhase.Idle;
    private float laserTimer = 0f;
    private float damageAccumulator = 0f;


    private bool isDashing = false;
    private Vector3 dashTarget;
    private Vector3 dashDirection;

    void Awake()
    {
        GetComponent<Health>().maxHealth = health;
        p_agent = GetComponent<NavMeshAgent>();
        m_player = GameObject.Find("PlayerManager").GetComponent<PlayerManager>().player;
        m_hitbox = hitboxObj.GetComponent<BoxCollider>();
        p_rb = GetComponent<Rigidbody>();
        p_agent.speed = speed;
        //gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
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


    }

    void Update()
    {
        m_slamcur -= Time.deltaTime;
        m_swingCur -= Time.deltaTime;
        m_laserCur -= Time.deltaTime;

        if (m_slamcur <= 0)
        {
            canSlam = true;
        }

        if (m_swingCur <= 0)
        {
            canSwing = true;
        }
        else canSwing = false;

        if (m_laserCur <= 0)
        {
            canLaser = true;
        }
        else canLaser = false;

        if (hitboxActive)
        {
            if (Physics.BoxCast(hitboxObj.transform.position, m_hitbox.size / 2.0f, transform.forward, out RaycastHit hitInfo, hitboxObj.transform.rotation, Vector3.Distance(hitboxObj.transform.localPosition, m_hitbox.center), layerMask))
            {

                if (!m_hitPlayer)
                {
                    DamagePlayer(swingDamage);
                }
            }
        }

        if (isDashing)
        {
            TurnOnHitBox();
            p_rb.MovePosition(Vector3.MoveTowards(transform.position, dashTarget, swingDashSpeed * Time.deltaTime));

            if (Vector3.Distance(transform.position, dashTarget) <= 0.1f)
            {
                isDashing = false;
            }
        }
        else ResetHitBox();


    }
    public void DamagePlayer(int _damage)
    {
        Debug.Log("HitPlayer");
        m_player.GetComponent<Health>().Damage(_damage);
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
        transform.position = centerPoint.position + slamOffset;
        SpawnAllObjects();
        canSlam = false;
        slamIsDone = true;

    }

    public void LaserBeam()
    {

        float delta = Time.deltaTime;
        switch (laserPhase)
        {
            case LaserPhase.Idle:
                laserFirePoint.transform.LookAt(m_player.transform);
                lr.enabled = true;
                lr.widthCurve = AnimationCurve.Constant(0, 1, telegraphWidth);
                laserTimer = laserWindupTime;
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
                if (Physics.Raycast(startW, dirW, out RaycastHit hitW, maxBeamDistance))
                {
                    lr.SetPosition(0, startW);
                    lr.SetPosition(1, hitW.point);
                }

                if (laserTimer <= 0f)
                {
                    laserTimer = laserFireTime;
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

                if (Physics.Raycast(startF, dirF, out RaycastHit hitF, maxBeamDistance, layerMask))
                {

                    lr.SetPosition(0, startF);
                    lr.SetPosition(1, hitF.point);

                    if (hitF.collider.CompareTag("Player"))
                    {
                        damageAccumulator += damagePerSecond * Time.deltaTime;
                        if (damageAccumulator >= 1f)
                        {
                            int applyDamage = Mathf.FloorToInt(damageAccumulator);
                            hitF.collider.GetComponent<Health>().Damage(applyDamage);
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
                    m_laserCur = laserCooldown;
                    canLaser = false;
                    laserPhase = LaserPhase.Idle;
                }
                return;
        }
    }

    private Vector3 GetAimDirection()
    {
        return laserFirePoint.transform.forward;
    }

    private void RotateTowardsTarget(float delta)
    {
        if (m_player == null) return;
        Vector3 toTarget = m_player.transform.position - laserFirePoint.transform.position;
        if (yawOnly) toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        float distance = Vector3.Distance(transform.position, m_player.transform.position);
        //Debug.Log(distance);
        scaledRotation = accuracyCurve.Evaluate(distance);
        //Debug.Log("distance" + distance);
        //Debug.Log("scaled Rot" + scaledRotation);

        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        laserFirePoint.transform.rotation = Quaternion.RotateTowards(laserFirePoint.transform.rotation,targetRot,scaledRotation * delta);
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


    public void StartSwingCombo()
    {
        dashDirection = (m_player.transform.position - transform.position).normalized;
        dashDirection.y = 0;
        DoNextSwing();
    }

    private void DoNextSwing()
    {
        dashDirection = (m_player.transform.position - transform.position).normalized;
        dashDirection.y = 0;
        dashTarget = transform.position + dashDirection * swingDashDistance;
        isDashing = true;
    }

    public void StartSwingCooldown()
    {
        m_swingCur = swingCooldown;
    }
    public void StopSwinging()
    {
        anim.SetBool("Swing", false);
    }

    public void Dead()
    {
        PlayerManager.instance.wallet.Add((int)Random.Range(moneyToAdd.x, moneyToAdd.y));
        GameManager.instance.bossDefeated = true;
        Destroy(gameObject);
    }
}
