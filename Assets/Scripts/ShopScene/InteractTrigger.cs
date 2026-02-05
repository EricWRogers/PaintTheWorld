using UnityEngine;
using TMPro;

public class InteractTrigger : MonoBehaviour
{
    public string promptText = "Press E";
    public GameObject promptObject; 
    public TMP_Text promptLabel;

    public MonoBehaviour stationToOpen; // PawnShop or Vending Machine
    private bool inRange;

    void Awake()
    {
        if (promptObject) promptObject.SetActive(false);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = true;

        if (promptObject) promptObject.SetActive(true);
        if (promptLabel) promptLabel.text = promptText;
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        inRange = false;

        if (promptObject) promptObject.SetActive(false);
    }

    void Update()
    {
        if (!inRange) return;
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (stationToOpen != null)
                stationToOpen.Invoke("Open", 0f);
        }
    }
}
