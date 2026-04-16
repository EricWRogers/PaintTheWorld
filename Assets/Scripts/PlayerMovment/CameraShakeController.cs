using System;
using System.Collections;
using TreeEditor;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    private CinemachineVirtualCamera virtualCamera;
    private CinemachineBasicMultiChannelPerlin noise;

    void Awake()
    {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();

    }

    public void ShakeCamera(float intensity, float duration)
    {
        noise.AmplitudeGain = intensity;
        StartCoroutine(WaitTime(duration));
        Invoke(nameof(ResetIntensity), duration);
    }

    IEnumerator WaitTime(float shakeTime)
    {
        yield return new WaitForSeconds(shakeTime);
    }

    void ResetIntensity()
    {
        noise.AmplitudeGain = 0f;
    }

}
