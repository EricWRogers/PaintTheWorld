using UnityEngine;

public class ShieldEnemy : Enemy
{
    public float bashSpeed;
    public float bashDuration;
    public bool m_isDashing;
    private float m_curDuration = 0;
    private Vector3 m_dashDirection;

    new void Awake()
    {
        base.Awake();
    }

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();
        m_curDuration -= Time.deltaTime;
        if (m_isDashing)
        {
            if (m_curDuration < 0)
            {
                p_rb.linearVelocity = Vector3.zero;
                m_isDashing = false;
            }
            else
            {
                p_rb.linearVelocity = m_dashDirection * bashSpeed;
            }
        }

    }
    public void ShieldBash()
    {
        m_isDashing = true;
        m_curDuration = bashDuration;
        m_dashDirection = (player.transform.position - transform.position).normalized;
        transform.LookAt(m_dashDirection);
        


    }
}
