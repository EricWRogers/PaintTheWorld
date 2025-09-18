using SuperPupSystems.Helper;
using SuperPupSystems.StateMachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[System.Serializable]
public class WalkToPlayer : SimpleState
{

    [Header("State Machine Gets These")]
    public SimpleStateMachine sm;
    public Transform target;
    public GameObject boss;
    public NavMeshAgent agent;

    private Boss m_boss;
    private enum LaserPhase { Idle, Windup, Firing, Cooldown }
    private LaserPhase laserPhase = LaserPhase.Idle;
    private float laserTimer = 0f;
    private float damageAccumulator = 0f;
    public override void OnStart()
    {
        m_boss = boss.GetComponent<Boss>();
        agent.SetDestination(target.position);
        agent.isStopped = false;
    }
    public override void UpdateState(float dt)
    {
        agent.SetDestination(target.position);
        float delta = Time.deltaTime;
        switch (laserPhase)
        {
            case LaserPhase.Idle:
                if (m_boss.canLaser)
                {
                    agent.isStopped = true;
                    m_boss.lr.enabled = true;
                    m_boss.lr.widthCurve = AnimationCurve.Constant(0, 1, m_boss.telegraphWidth);
                    laserTimer = m_boss.windupTime;
                    laserPhase = LaserPhase.Windup;
                }
                break;

            case LaserPhase.Windup:
                laserTimer -= delta;
                RotateTowardsTarget(delta);

                float alpha = Mathf.PingPong(Time.time * 2f, 1f);
                SetLineRendererForTelegraph(alpha);

                Vector3 startW = m_boss.laserFirePoint.transform.position;
                Vector3 dirW = GetAimDirection();
                m_boss.lr.SetPosition(0, startW);
                m_boss.lr.SetPosition(1, startW + dirW * m_boss.maxBeamDistance);

                if (laserTimer <= 0f)
                {
                    laserTimer = m_boss.fireTime;
                    m_boss.lr.widthCurve = AnimationCurve.Constant(0, 1, m_boss.beamWidth);
                    SetLineRendererGradient(m_boss.firingGradient);
                    laserPhase = LaserPhase.Firing;
                }
                break;

            case LaserPhase.Firing:
                laserTimer -= delta;
                RotateTowardsTarget(delta);

                Vector3 startF = m_boss.laserFirePoint.transform.position;
                Vector3 dirF = GetAimDirection();

                if (Physics.Raycast(startF, dirF, out RaycastHit hit, m_boss.maxBeamDistance, m_boss.layerMask))
                {
                    
                    m_boss.lr.SetPosition(0, startF);
                    m_boss.lr.SetPosition(1, hit.point);

                    if (hit.collider.CompareTag("Player"))
                    {
                        damageAccumulator += m_boss.damagePerSecond * Time.deltaTime;
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
                    m_boss.lr.SetPosition(0, startF);
                    m_boss.lr.SetPosition(1, startF + dirF * m_boss.maxBeamDistance);
                }

                if (laserTimer <= 0f)
                {
                    m_boss.lr.enabled = false;
                    agent.isStopped = false;
                    laserTimer = m_boss.laserCooldown;
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
    public override void OnExit()
    {
        base.OnExit();
        agent.isStopped = true;

    }

    private Vector3 GetAimDirection()
    {
        return m_boss.laserFirePoint.transform.forward;
    }

    private void RotateTowardsTarget(float delta)
    {
        if (target == null) return;
        Vector3 toTarget = target.position - m_boss.laserFirePoint.transform.position;
        if (m_boss.yawOnly) toTarget.y = 0;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
        m_boss.laserFirePoint.transform.rotation = Quaternion.RotateTowards(m_boss.laserFirePoint.transform.rotation, targetRot, m_boss.laserRotationSpeed * delta);
    }
    
    void SetLineRendererForTelegraph(float alpha)
    {
        // Create temporary gradient from telegraphGradient but muting alpha via parameter
        Gradient g = new Gradient();
        GradientColorKey[] colorKeys = m_boss.telegraphGradient.colorKeys;
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[colorKeys.Length];
        for (int i = 0; i < colorKeys.Length; i++)
        {
            alphaKeys[i].alpha = m_boss.telegraphGradient.Evaluate(colorKeys[i].time).a * alpha;
            alphaKeys[i].time = colorKeys[i].time;
        }
        g.SetKeys(colorKeys, alphaKeys);
        m_boss.lr.colorGradient = g;
    }

    void SetLineRendererGradient(Gradient source)
    {
        Gradient g = new Gradient();
        g.SetKeys(source.colorKeys, source.alphaKeys);
        m_boss.lr.colorGradient = g;
    }
    
}
