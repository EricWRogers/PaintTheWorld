using UnityEngine;
using TMPro;
using System;
using Unity.VisualScripting;

public class PerformanceMonitor : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text text;

    [Header("Sampling")]
    public float sampleDuration = 1f;
    public int maxSamples = 10000;

    private float[] frameTimes;
    private int sampleCount;
    private float timer;
    private bool isActive = false;



    void Awake()
    {
        frameTimes = new float[maxSamples];
        if(!isActive)
        text.GameObject().SetActive(false);
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.F3))
        {
            isActive = !isActive;
            text.GameObject().SetActive(isActive);
        }
        float dt = Time.unscaledDeltaTime;

        if (sampleCount < maxSamples)
            frameTimes[sampleCount++] = dt;

        timer += dt;

        if (timer >= sampleDuration)
        {
            CalculateAndDisplay();
            timer = 0f;
            sampleCount = 0;
        }
    }

    void CalculateAndDisplay()
    {
        if (sampleCount == 0) return;

        float total = 0f;
        for (int i = 0; i < sampleCount; i++)
            total += frameTimes[i];

        float avgFPS = sampleCount / total;


        float[] temp = new float[sampleCount];
        Array.Copy(frameTimes, temp, sampleCount);

        Array.Sort(temp); // ascending (fast frames first)

        int onePercentIndex = Mathf.Clamp((int)(sampleCount * 0.99f), 0, sampleCount - 1);
        int pointOnePercentIndex = Mathf.Clamp((int)(sampleCount * 0.999f), 0, sampleCount - 1);

        float onePercentLowFPS = 1f / temp[onePercentIndex];
        float pointOnePercentLowFPS = 1f / temp[pointOnePercentIndex];



        text.text =
            $"Avg FPS: {avgFPS:F1}\n" +
            $"1% Low: {onePercentLowFPS:F1}\n" +
            $"0.1% Low: {pointOnePercentLowFPS:F1}\n\n"; 
    }
}