using UnityEngine;
using TMPro; 

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private Currency wallet;
    [SerializeField] private TMP_Text label;

    private void Awake()
    {
        if (!wallet) wallet = FindObjectOfType<Currency>();
    }

    private void OnEnable()
    {
        if (wallet != null)
        {
            wallet.changed.AddListener(OnChanged);
            OnChanged(wallet.amount); // initialize
        }
    }

    private void OnDisable()
    {
        if (wallet != null) wallet.changed.RemoveListener(OnChanged);
    }

    private void OnChanged(int amt)
    {
        if (label) label.text = $"$ {amt}";
    }
}