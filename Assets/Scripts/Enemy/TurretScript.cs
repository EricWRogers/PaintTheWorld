using SuperPupSystems.Helper;
using UnityEngine;

public class TurretScript : MonoBehaviour
{
    public EnemyManager EM;
    public int health;
    [Header("laser targeting")]
    public GameObject laserFirePoint;
    public float laserRotationSpeed = 360f;
    public bool yawOnly = true;
    public LayerMask layerMask;

    [Header("laser Timers")]
    public float windupTime = 1.2f;
    public float fireTime = 2.0f;
    public float laserCooldown;
    public bool SingleFire;

    [Header("laser stats")]
    public float maxBeamDistance = 50f;
    public float beamWidth = 0.12f;
    public int damage;
    public bool isLasering;

    [Header("laser Visuals")]
    public Gradient telegraphGradient;
    public Gradient firingGradient;
    public float telegraphWidth = 0.08f;

    public LineRenderer lr;

    private enum LaserPhase { Idle, Windup, Firing, Cooldown }
    private LaserPhase laserPhase = LaserPhase.Idle;
    private float laserTimer = 0f;
    private float damageAccumulator = 0f;
    private GameObject m_player;

    void Awake()
    {
        m_player = GameObject.FindGameObjectWithTag("Player");
        GetComponent<Health>().maxHealth = health;
    }
    void Start()
    {
        EM = GameObject.Find("EnemyManager").GetComponent<EnemyManager>();
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

    void Update()
    {
        LaserBeam();
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
                laserTimer = windupTime;
                laserPhase = LaserPhase.Windup;
                break;

            case LaserPhase.Windup:
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

                if (Physics.Raycast(startF, dirF, out RaycastHit hitF, maxBeamDistance))
                {

                    lr.SetPosition(0, startF);
                    lr.SetPosition(1, hitF.point);

                    if (hitF.collider.CompareTag("Player"))
                    {
                        if (SingleFire)
                        {
                            hitF.collider.GetComponent<Health>().Damage(damage);
                            laserTimer = 0f;
                        }
                        else
                        {
                            damageAccumulator += damage * Time.deltaTime;
                            if (damageAccumulator >= 1f)
                            {
                                int applyDamage = Mathf.FloorToInt(damageAccumulator);
                                hitF.collider.GetComponent<Health>().Damage(applyDamage);
                                damageAccumulator -= applyDamage;
                            }
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
                    laserTimer = laserCooldown;
                    laserPhase = LaserPhase.Cooldown;
                }
                break;

            case LaserPhase.Cooldown:
                laserFirePoint.transform.LookAt(m_player.transform);
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
        if (m_player == null) return;
        Vector3 toTarget = m_player.transform.position - laserFirePoint.transform.position;
        if (yawOnly) toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        float distance = toTarget.magnitude;
        float scaledRotation = laserRotationSpeed / (1f + distance * distance * 0.01f);
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
    public void Dead()
    {
        Destroy(gameObject);
    }
    public void OnDestroy()
    {
        EM.RemoveEnemy();
    }
}
