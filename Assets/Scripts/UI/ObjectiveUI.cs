using TMPro;
using UnityEngine;

public class ObjectiveUI : MonoBehaviour
{
    public TMP_Text objectiveText;
    public string captureText = "Capture Posters";
    public string holdText = "Hold Posters";

    void Update()
    {
        if(GameManager.instance.currentGamemode == GameManager.gameModes.HoldPoints)
        {
            objectiveText.text = holdText;
        }
        if(GameManager.instance.currentGamemode == GameManager.gameModes.CapturePoints)
        {
            objectiveText.text = captureText;
        }
    }
}
