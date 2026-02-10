using UnityEngine;

public class PaintingObj : MonoBehaviour
{
    private Paintable m_paintable;
    public int coinsGained;
    public float coinGainDelay = 3;
    private float m_timer;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_paintable = GetComponent<Paintable>();
    }

    // Update is called once per frame
    void Update()
    {
        if(m_paintable.covered)
        {
            m_timer -= Time.deltaTime;
            if(m_timer <= 0)
            {
                PlayerManager.instance.wallet.Add(coinsGained);
                m_timer = coinGainDelay;
            }
        }
    }
}
