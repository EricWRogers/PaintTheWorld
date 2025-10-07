using SuperPupSystems.Helper;
using UnityEngine;

public class TurretScript : Enemy
{
    [Header("laser targeting")]
    public float scaledRotation;
    public bool yawOnly = true;
    public AnimationCurve accuracyCurve;

    [Header("laser Timers")]
    public float windupTime = 1.2f;
    public float fireTime = 2.0f;
    public float laserCooldown;

    [Header("laser stats")]
    public float maxBeamDistance = 50f;
    public float beamWidth = 0.12f;
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

    new void Start()
    {
        base.Start();
        firePoint.transform.LookAt(player.transform);
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
        if (player == null)
        {
            return;
        }
        if (Vector3.Distance(player.transform.position, transform.position) <= maxBeamDistance)
        {
            Attack();
        }
            
    }


    public override void Attack()
    {
        float delta = Time.deltaTime;
        switch (laserPhase)
        {
            case LaserPhase.Idle:
                RotateTowardsTarget(delta);
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

                Vector3 startW = firePoint.transform.position;
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

                Vector3 startF = firePoint.transform.position;
                Vector3 dirF = GetAimDirection();

                if (Physics.Raycast(startF, dirF, out RaycastHit hitF, maxBeamDistance))
                {

                    lr.SetPosition(0, startF);
                    lr.SetPosition(1, hitF.point);

                    if (hitF.collider.CompareTag("Player"))
                    {
                        damageAccumulator += baseDamage * Time.deltaTime;
                        if (damageAccumulator >= 1f)
                        {
                            int applyDamage = Mathf.FloorToInt(damageAccumulator);
                            PlayerManager.instance.health.Damage(applyDamage);
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
                    laserTimer = laserCooldown;
                    laserPhase = LaserPhase.Cooldown;
                }
                break;

            case LaserPhase.Cooldown:
                RotateTowardsTarget(delta);
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
        return firePoint.transform.forward;
    }

    private void RotateTowardsTarget(float delta)
    {
        if (player == null) return;
        Vector3 toTarget = player.transform.position - firePoint.transform.position;
        if (yawOnly) toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        float distance = Vector3.Distance(transform.position, player.transform.position);
        //Debug.Log(distance);
        scaledRotation = accuracyCurve.Evaluate(distance);
        //Debug.Log("distance" + distance);
        //Debug.Log("scaled Rot" + scaledRotation);

        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        firePoint.transform.rotation = Quaternion.RotateTowards(firePoint.transform.rotation,targetRot,scaledRotation * delta);
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
}
