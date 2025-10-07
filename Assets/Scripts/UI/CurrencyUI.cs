using UnityEngine;
using TMPro; 

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private Currency wallet;
    [SerializeField] private TMP_Text label;

    private void Start()
    {
        wallet = PlayerManager.instance.wallet;
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
    void Update()
    {
        if (wallet == null)
        {
            //wallet = PlayerManager.instance.wallet;
        }
    }

    private void OnChanged(int amt)
    {
        if (label) label.text = $"$ {amt}";
    }
}