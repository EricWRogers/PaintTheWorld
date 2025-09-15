using UnityEngine;

public class ShieldEnemy : Enemy
{
    public float bashSpeed;
    public float bashDuration;
    private bool m_isDashing;
    private float m_curDuration = 0;

    void Update()
    {
        m_curDuration -= Time.deltaTime;
        if (m_isDashing)
        {
            if (m_curDuration < 0)
            {
                p_rb.linearVelocity = Vector3.zero;
                m_isDashing = false;
            }
        }
        
    }
    public void ShieldBash()
    {
        m_isDashing = true;
        m_curDuration = bashDuration;
        Vector3 dir = (player.transform.position - transform.position).normalized;
        transform.LookAt(dir);
        p_rb.linearVelocity = dir * bashSpeed;


    }
}
