using UnityEngine;
using System;

public class KioskCameraController : MonoBehaviour
{
    public static KioskCameraController I;

    public Camera cam;
    private Transform originalParent;
    private Vector3 originalPos;
    private Quaternion originalRot;

    public bool inKiosk;

    void Awake()
    {
        I = this;
        if (!cam) cam = Camera.main;
        originalPos = cam.transform.position;
        originalRot = cam.transform.rotation;
    }

    public void EnterKiosk(Transform target, Action onArrive = null)
    {
        if (!cam || !target) { onArrive?.Invoke(); return; }

        inKiosk = true;

        
        originalPos = cam.transform.position;
        originalRot = cam.transform.rotation;

        cam.transform.position = target.position;
        cam.transform.rotation = target.rotation;

        onArrive?.Invoke();
    }

    public void ExitKiosk(Action onExit = null)
    {
        if (!cam) { onExit?.Invoke(); return; }

        inKiosk = false;
        cam.transform.position = originalPos;
        cam.transform.rotation = originalRot;

        onExit?.Invoke();
    }
}
