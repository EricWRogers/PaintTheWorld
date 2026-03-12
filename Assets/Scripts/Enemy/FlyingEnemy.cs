using System.Collections.Generic;
using UnityEngine;
using SuperPupSystems.Helper;
public class FlyingEnemy : MonoBehaviour
{
    public CloudNav cloudNav;
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float fireRange;
    public float minStopDistance;
    public int damage;
    public float fireRate;
    private float m_curFireTime;
    private GameObject m_player;
    public List<Vector3> path;
    public int startId;
    public int endId;
    public int targetIndex;
    public float speed = 100.0f;
    public bool stopped = false;

    Vector3 ZeroY(Vector3 _vector)
    {
        return new Vector3(_vector.x, 0.0f, _vector.z);
    }

    void Start()
    {
        m_player = PlayerManager.instance.player;
        RequestNewPath();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (path.Count == 0)
        {
            RequestNewPath();
            return;
        }

        float distance = Vector3.Distance(transform.position, m_player.transform.position);

        if (distance <= minStopDistance)
        {
            stopped = true;
        }

        if (distance <= fireRange && stopped)
        {
            transform.LookAt(m_player.transform.position);
            Shoot();
        }
        else
        {
            stopped = false;
            if (transform.position == path[targetIndex])
            {
                targetIndex++;

                RequestNewPath();
                return;
            }

            Vector3 direction = (path[targetIndex] - transform.position).normalized;
            Vector3 movePosition = transform.position + (direction * speed * Time.fixedDeltaTime);

            transform.LookAt(movePosition);

            if (Vector3.Distance(transform.position, path[targetIndex]) < Vector3.Distance(transform.position, movePosition))
            {
                transform.position = path[targetIndex];
                return;
            }

            transform.position = movePosition;
        }


    }

    public void Shoot()
    {
        if (m_curFireTime > fireRate * 0.5f)
            transform.LookAt(m_player.transform);
        
        m_curFireTime -= Time.fixedDeltaTime;
        if (m_curFireTime <= 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
            bullet.GetComponent<Bullet>().damage = damage;
            m_curFireTime = fireRate;
        }

    }

    void RequestNewPath()
    {
        startId = cloudNav.aStar.GetClosestPoint(transform.position);
        endId = cloudNav.aStar.GetClosestPoint(m_player.transform.position);

        cloudNav.aStar.RequestPath(GetNewPath, startId, endId);
    }

    void GetNewPath(List<Vector3> _path)
    {
        path.Clear();

        targetIndex = 0;

        path = _path;
    }
    
    public void Dead()
    {
        
    }
}
