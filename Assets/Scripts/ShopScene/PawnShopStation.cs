using UnityEngine;

public class PawnShopStation : MonoBehaviour
{
    public PawnShopUI ui;
    [Range(0f,1f)] public float swapChance = 0.75f;
    private bool usedThisScene = false;

    void OnTriggerStay(Collider other)
    {
        if (usedThisScene) return;
        if (!other.CompareTag("Player")) return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            ui.Open(
                onSwapped: ()=> usedThisScene = true,
                chanceProvider: ()=> Random.value <= swapChance
            );
        }
    }
}
