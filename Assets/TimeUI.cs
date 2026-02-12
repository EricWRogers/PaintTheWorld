using UnityEngine;
using TMPro; // Remove if using standard Unity Text
using SuperPupSystems.Helper;

public class TimerUI : MonoBehaviour
{
    [Header("References")]
    public Timer timerLogic; // Drag the object with your Timer script here
    public TextMeshProUGUI timerText; // Drag your UI Text here

    [Header("Settings")]
    public string prefix = "Time: ";
    public bool showMilliseconds = false;

    void Update()
    {
        if (timerLogic != null && timerText != null)
        {
            UpdateDisplay();
        }
    }

    void UpdateDisplay()
    {
        float timeToDisplay = timerLogic.timeLeft;

        // Prevent negative display
        if (timeToDisplay < 0) timeToDisplay = 0;

        // Calculate Minutes and Seconds
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        if (showMilliseconds)
        {
            float fractional = (timeToDisplay * 100) % 100;
            timerText.text = string.Format("{0}{1:00}:{2:00}:{3:00}", prefix, minutes, seconds, fractional);
        }
        else
        {
            timerText.text = string.Format("{0}{1:00}:{2:00}", prefix, minutes, seconds);
        }
    }
}