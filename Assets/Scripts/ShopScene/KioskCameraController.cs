using UnityEngine;
using System.Collections;

public class KioskCameraController : MonoBehaviour
{
    public static KioskCameraController I;

    [Header("Camera")]
    public Camera cam;
    public float moveTime = 0.35f;

    Transform originalParent;
    Vector3 originalPos;
    Quaternion originalRot;

    bool inKiosk;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        if (!cam) cam = Camera.main;
    }

    public bool IsInKiosk => inKiosk;

    public void EnterKiosk(Transform cameraTarget, System.Action onArrived)
    {
        if (inKiosk) return;
        StartCoroutine(EnterRoutine(cameraTarget, onArrived));
    }

    public void ExitKiosk(System.Action onExited)
    {
        if (!inKiosk) return;
        StartCoroutine(ExitRoutine(onExited));
    }

    IEnumerator EnterRoutine(Transform target, System.Action onArrived)
    {
        inKiosk = true;

        
        originalParent = cam.transform.parent;
        originalPos = cam.transform.position;
        originalRot = cam.transform.rotation;

        
        PlayerInputLock.SetLocked(true);

        float t = 0f;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, moveTime);
            cam.transform.position = Vector3.Lerp(startPos, target.position, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, target.rotation, t);
            yield return null;
        }

        onArrived?.Invoke();
    }

    IEnumerator ExitRoutine(System.Action onExited)
    {
        float t = 0f;
        Vector3 startPos = cam.transform.position;
        Quaternion startRot = cam.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, moveTime);
            cam.transform.position = Vector3.Lerp(startPos, originalPos, t);
            cam.transform.rotation = Quaternion.Slerp(startRot, originalRot, t);
            yield return null;
        }

        cam.transform.SetParent(originalParent);

        PlayerInputLock.SetLocked(false);
        inKiosk = false;

        onExited?.Invoke();
    }
}
