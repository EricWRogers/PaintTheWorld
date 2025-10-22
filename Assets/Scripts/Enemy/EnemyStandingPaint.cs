using SuperPupSystems.Helper;
using UnityEngine;

public class EnemyStandingPaint : GetPaintColor
{
    public float dOTRate = 1;
    public int paintDamage = 2;
    private float m_timer;
    void Update()
    {
        m_timer -= Time.deltaTime;
        CheckOnPaint();
        if(standingColor == PaintManager.instance.GetComponent<PaintColors>().damagePaint)
        {
            if(m_timer <= 0)
            {
                GetComponent<Health>().Damage(paintDamage);
                m_timer = dOTRate;
            }
            
        }
    }
}
