using UnityEngine;
using System.Collections;

public class AmmoStation : MonoBehaviour
{
    [Header("Settings")]
    public int refillAmount = 50;        
    public float cooldownTime = 5f; // Shortened for testing    
    
    [Header("References")]
    // MAKE SURE: This is a child object, not the object holding this script!
    public GameObject ammoPickupVisual; 
    
    [Header("Animation Settings")]
    public float rotationSpeed = 100f;
    public float bobSpeed = 2f;
    public float bobHeight = 0.2f;

    private bool isReady = true;
    private Vector3 startPos;

    private void Start()
    {
        if (ammoPickupVisual != null)
            startPos = ammoPickupVisual.transform.localPosition;
    }

    private void Update()
    {
        if (isReady && ammoPickupVisual != null)
        {
            ammoPickupVisual.transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
            float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            ammoPickupVisual.transform.localPosition = new Vector3(startPos.x, newY, startPos.z);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Debug to see if the trigger even fires
        Debug.Log("Object entered trigger: " + other.name);

        if (isReady && other.CompareTag("Player"))
        {
            // Search the player and all children for the SprayPaint script
            SprayPaint weapon = other.GetComponentInChildren<SprayPaint>();

            if (weapon != null)
            {
                weapon.AddAmmo(refillAmount);
                StartCoroutine(RespawnRoutine());
            }
            else
            {
                // If this hits, the script isn't on the player or their held items
                Debug.LogError("Found Player, but SprayPaint script is missing!");
            }
        }
    }

    private IEnumerator RespawnRoutine()
    {
        isReady = false;
        
        if (ammoPickupVisual != null) 
            ammoPickupVisual.SetActive(false); 

        // This will now survive because 'this' GameObject stays active
        yield return new WaitForSeconds(cooldownTime);

        isReady = true;
        if (ammoPickupVisual != null) 
            ammoPickupVisual.SetActive(true);
            
        Debug.Log("Station is ready again!");
    }
}