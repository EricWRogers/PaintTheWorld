using SuperPupSystems.Helper;
using UnityEngine;

public class MortarShell : MonoBehaviour
{
    public Vector3 startPos;
    public Vector3 endPos;
    public float flightTime = 2f;
    public float arcHeight = 3f;
    public int damage;
    public float radius;
    public LayerMask layerMask;
    private float timer = 0f;
    private bool isFlying = false;

    public void Launch(Vector3 start, Vector3 end, float time, float height)
    {
        startPos = start;
        endPos = end;
        flightTime = time;
        arcHeight = height;
        timer = 0f;
        isFlying = true;
        transform.position = startPos;
    }

    void Update()
    {
        if (!isFlying) return;

        timer += Time.deltaTime;
        float t = Mathf.Clamp01(timer / flightTime);

        Vector3 horizontalPos = Vector3.Lerp(startPos, endPos, t);
        float heightOffset = arcHeight * 4f * t * (1 - t);
        transform.position = horizontalPos + Vector3.up * heightOffset;

        if (t >= 1f)
        {
            isFlying = false;
            OnImpact();
        }
    }

    private void OnImpact()
    {
        if (Physics.OverlapSphere(transform.position, radius, layerMask).Length > 0)
        {
            Health health = PlayerManager.instance.health;
            if (health != null)
            {
                health.Damage(damage);
                Destroy(gameObject);
            }
        }
        Destroy(gameObject);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}