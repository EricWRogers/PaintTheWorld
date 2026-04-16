using System;
using System.Collections;
using TreeEditor;
using Unity.Cinemachine;
using UnityEngine;

public class CameraShakeController : MonoBehaviour
{
    public CinemachineCamera virtualCamera;
    public CinemachineImpulseSource impulseSource;

    void Awake()
    {


    }

    void Start()
    {
        impulseSource = virtualCamera.GetComponent<CinemachineImpulseSource>();
        impulseSource.GenerateImpulse();
    }
    


}
