using UnityEngine;
using Unity.Cinemachine;

public class KioskCameraController : MonoBehaviour
{
    public static KioskCameraController I;

    [Header("Cameras")]
    public CinemachineCamera pawnShopCam;   

    public CinemachineCamera freeLookCam;       

    [Header("Player")]
    public MonoBehaviour playerMovementScript;    

    private int pawnCamDefaultPriority;
    private int freeLookDefaultPriority;

    void Awake()
    {
        I = this;
        if (pawnShopCam) pawnCamDefaultPriority = pawnShopCam.Priority;
        if (freeLookCam) freeLookDefaultPriority = freeLookCam.Priority;
    }

    public void EnterKiosk(Transform target, System.Action onEntered = null)
    {
        // lock movement
        if (playerMovementScript) playerMovementScript.enabled = false;

      
        if (pawnShopCam)
        {
            pawnShopCam.Follow = target;
            pawnShopCam.LookAt = target;
            pawnShopCam.Priority = 20;
        }

        if (freeLookCam) freeLookCam.Priority = 0;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        onEntered?.Invoke();
    }

    public void ExitKiosk()
    {
        if (playerMovementScript) playerMovementScript.enabled = true;

        if (pawnShopCam) pawnShopCam.Priority = pawnCamDefaultPriority;
        if (freeLookCam) freeLookCam.Priority = freeLookDefaultPriority;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
