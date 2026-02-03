using UnityEngine;

public class GlobLauncher : MonoBehaviour
{
    // Prefab to spawn (should represent the projectile)
    public GameObject projectilePrefab;

    // Where the projectile appears and what forward direction to use
    public Transform spawnPoint;

    // Force applied to the projectile when launched
    public float launchForce = 800f;

    // Time after which spawned projectile is destroyed
    public float projectileLifetime = 5f;

    private float cooldownTime = 1f;
    private float lastLaunchTime = -Mathf.Infinity;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Time.time >= lastLaunchTime + cooldownTime)
            {
                Shoot();
                lastLaunchTime = Time.time;
            }
        }

    }

    void Shoot()
    {
        if (projectilePrefab == null || spawnPoint == null)
            return;

        GameObject proj = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);

        Rigidbody rb = proj.GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = proj.AddComponent<Rigidbody>();
        }

        rb.AddForce(spawnPoint.forward * launchForce);

        Destroy(proj, projectileLifetime);
    }
}
