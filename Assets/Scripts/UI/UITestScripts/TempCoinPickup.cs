using UnityEngine;

public class TempCoinPickup : MonoBehaviour
{
    public int amount = 1;
    public string playerTag = "Player";

    private void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag)) return;

        var wallet = other.GetComponent<Currency>();
        if (!wallet) wallet = other.GetComponentInChildren<Currency>();
        if (wallet)
        {
            wallet.Add(amount);
            Destroy(gameObject);
        }
    }
}
