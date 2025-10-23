using TMPro;
using UnityEngine;

public class WaveCounter : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    void Update()
    {
        counterText.text = GameManager.instance.currentWave.ToString();
    }
}
