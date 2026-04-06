using UnityEngine;
using TMPro;
using SuperPupSystems.Helper;

public class TimerUI : MonoBehaviour
{
    [Header("References")]
    public Timer timerLogic;
    public TextMeshProUGUI timerText;

    [Header("Settings")]
    public string prefix = "Time: ";
    public bool showMilliseconds = false;

    [Header("Warning UI")]
    public float warningTime = 10f;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;

    [Header("Pulse Settings")]
    public float pulseSpeed = 6f;
    public float pulseAmount = 0.15f;

    private Vector3 originalScale;

    void Start()
    {
        if (timerText != null)
        {
            originalScale = timerText.transform.localScale;
        }
    }

    void Update()
    {
        if (timerLogic != null && timerText != null)
        {
            UpdateDisplay();
            UpdateWarningEffects();
        }
    }

    void UpdateDisplay()
    {
        float timeToDisplay = timerLogic.timeLeft;

        if (timeToDisplay < 0)
            timeToDisplay = 0;

        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        if (showMilliseconds)
        {
            int fractional = Mathf.FloorToInt((timeToDisplay * 100) % 100);
            timerText.text = string.Format("{0}{1:00}:{2:00}:{3:00}", prefix, minutes, seconds, fractional);
        }
        else
        {
            timerText.text = string.Format("{0}{1:00}:{2:00}", prefix, minutes, seconds);
        }
    }

    void UpdateWarningEffects()
    {
        if (timerLogic.timeLeft <= warningTime)
        {
            timerText.color = warningColor;

            float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            timerText.transform.localScale = originalScale * pulse;
        }
        else
        {
            timerText.color = normalColor;
            timerText.transform.localScale = originalScale;
        }
    }
}