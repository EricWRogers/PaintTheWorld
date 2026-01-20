using UnityEngine;
using System.Collections;

public class NewBrushWeapon : MonoBehaviour
{
    [Header("Ammo Settings")]
    public int maxAmmo = 100;
    public int currentAmmo = 50;

    [Header("Projectile Settings")]
    public GameObject globPrefab;
    public Transform spawnPoint;
    public float globForwardForce = 10f;

    // Call this to refill ammo from the station
    public void AddAmmo(int amount)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);
        Debug.Log($"Ammo refilled! Current Ammo: {currentAmmo}");
    }

    public void StartAttack(float animationDuration, int ammoToFire)
    {
        // Check if player has enough ammo; if not, fire what's left
        int actualAmmoToFire = Mathf.Min(ammoToFire, currentAmmo);

        if (actualAmmoToFire <= 0) 
        {
            Debug.Log("Out of ammo!");
            return;
        }
        
        // Use percentage-based calculation
        float spawnInterval = animationDuration / actualAmmoToFire;

        StartCoroutine(FireGlobsRoutine(actualAmmoToFire, spawnInterval));
    }

    private IEnumerator FireGlobsRoutine(int count, float interval)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnGlob();
            currentAmmo--; // Spend 1 ammo per glob
            yield return new WaitForSeconds(interval);
        }
    }

    private void SpawnGlob()
    {
        GameObject glob = Instantiate(globPrefab, spawnPoint.position, spawnPoint.rotation);
        Rigidbody rb = glob.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.AddForce(spawnPoint.forward * globForwardForce, ForceMode.Impulse);
        }
    }
}