using TMPro;
using UnityEngine;

public class WaveCounter : MonoBehaviour
{
    public TextMeshProUGUI counterText;
    void Update()
    {
        counterText.text = "wave " + GameManager.instance.currentWave.ToString() + "/" + GameManager.instance.totalWave.ToString();
    }
}
