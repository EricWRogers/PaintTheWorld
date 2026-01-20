using UnityEngine;
using System.Collections;

public class AmmoStation : MonoBehaviour
{
    public int refillAmount = 50;        // How much ammo to give
    public float cooldownTime = 30f;    // 30 second cooldown
    private bool isReady = true;        // Tracks if the station can be used
    private Animation anim;

    public GameObject stationVisual; 

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering has a "Player" tag and the station is ready
        if (other.CompareTag("Player") && isReady)
        {
            RefillPlayer(other.gameObject);
        }
    }

    private void RefillPlayer(GameObject player)
    {
        // Assuming your player has a script that manages ammo
        NewBrushWeapon weapon = player.GetComponentInChildren<NewBrushWeapon>();

        if (weapon != null)
        { 
            Debug.Log("Ammo Refilled!");
            StartCoroutine(CooldownRoutine());
        }
    }

    private IEnumerator CooldownRoutine()
    {
        isReady = false;

        anim = GetComponent<Animation>();
        
        if (stationVisual != null)
        {
            stationVisual.SetActive(false);
        }
        

        yield return new WaitForSeconds(cooldownTime);

        isReady = true;
        
        if (stationVisual != null) stationVisual.SetActive(true);
        Debug.Log("Ammo Station Ready Again!");
        anim.Play();
    }
}