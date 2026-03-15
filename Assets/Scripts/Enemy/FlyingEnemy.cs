using System.Collections.Generic;
using UnityEngine;
using SuperPupSystems.Helper;
public class FlyingEnemy : Enemy
{
    public CloudNav cloudNav;
    public float minStopDistance;
    public float minHeightFromPLayerToShoot;
    private float m_curFireTime;
    private GameObject m_player;
    public List<Vector3> path;
    public int startId;
    public int endId;
    public int targetIndex;
    public float speed = 100.0f;
    public bool stopped = false;

    public bool stunned;
    public float stunTime;
    private float m_stunTimer;

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
        if (stunned)
        {
            m_stunTimer -= Time.deltaTime;
            if(m_stunTimer <= 0)
            {
                modelMeshRenderer.materials[1].color = Color.clear;
                stopped = false;
                GetComponent<Health>().Revive();
                stunned = false;
            }
            return;
        }
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

        if (distance <= attackRange && stopped)
        {
            transform.LookAt(m_player.transform.position);
            Attack();
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

    void RequestNewPath()
    {
        float heightDifference = transform.position.y - m_player.transform.position.y;
        Vector3 offset = new Vector3(0, heightDifference, 0);
        startId = cloudNav.aStar.GetClosestPoint(transform.position);
        endId = cloudNav.aStar.GetClosestPoint(m_player.transform.position - offset);

        cloudNav.aStar.RequestPath(GetNewPath, startId, endId);
    }

    void GetNewPath(List<Vector3> _path)
    {
        path.Clear();

        targetIndex = 0;

        path = _path;
    }
    public void Stun()
    {
        stopped = true;
        modelMeshRenderer.materials[1].color = stunColor;
        m_stunTimer = stunTime;
        stunned = true;
    }

    public override void Attack()
    {
        if (m_curFireTime > attackSpeed * 0.5f)
            transform.LookAt(m_player.transform);
        
        m_curFireTime -= Time.fixedDeltaTime;
        if (m_curFireTime <= 0)
        {
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, transform.rotation);
            bullet.GetComponent<Bullet>().damage = baseDamage;
            m_curFireTime = attackSpeed;
        }
    }
}
