using TMPro;
using UnityEngine;

public class FloatingDamageNumbers : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float lifeTime = 0.6f;
    public float minDist = 2f;
    public float maxDist = 3;

    private Vector3 m_iniPos;
    private Vector3 m_targetPos;
    private float m_timer;

    void Start()
    {
        transform.LookAt(2 * transform.position - Camera.main.transform.position);

        float direction = Random.rotation.eulerAngles.z;
        m_iniPos = transform.position;
        float dist = Random.Range(minDist, maxDist);
        m_targetPos = m_iniPos + (Quaternion.Euler(0, 0, direction) * new Vector3(dist, dist, 0f));
        transform.localScale = Vector3.zero;
    }

    void Update()
    {
        m_timer += Time.deltaTime;
        float fraction = lifeTime / 2f;

        if (m_timer > lifeTime) Destroy(gameObject);
        else if (m_timer > fraction) text.color = Color.Lerp(text.color, Color.clear, (m_timer - fraction) / (lifeTime - fraction));

        transform.position = Vector3.Lerp(m_iniPos, m_targetPos, Mathf.Sin(m_timer / lifeTime));
        transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, Mathf.Sin(m_timer / lifeTime));
    }
    
    public void SetDamageText(int damage)
    {
        text.text = damage.ToString();
    }
}
