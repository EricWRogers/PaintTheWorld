using UnityEngine;

public class DebugCurrencyInput : MonoBehaviour
{
    public Currency wallet;

    private void Awake()
    {
        if (!wallet) wallet = GetComponent<Currency>();
    }

    private void Update()
    {
        if (!wallet) return;

        if (Input.GetKeyDown(KeyCode.C)) wallet.Add(5);     // +5
        if (Input.GetKeyDown(KeyCode.V)) wallet.Spend(2);   // -2 if possible
    }
}
