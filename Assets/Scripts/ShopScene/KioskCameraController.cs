using UnityEngine;
using Unity.Cinemachine;
using UnityEngine.InputSystem;

public class KioskCameraController : MonoBehaviour
{
    public static KioskCameraController I;

    [Header("Cameras")]
    public CinemachineCamera kioskCam;   

    public CinemachineCamera freeLookCam;       

     [Header("Player Lock")]
    public MonoBehaviour playerMovementScript;   
    public PlayerInput playerInput;              
    public string gameplayActionMap = "Player"; 
    public string kioskActionMap = "UI";         

    [Header("HUD")]
    public GameObject hudRoot;                   

    int kioskDefaultPriority, freeLookDefaultPriority;

    void Awake()
    {
        I = this;
        if (kioskCam) kioskDefaultPriority = kioskCam.Priority;
        if (freeLookCam) freeLookDefaultPriority = freeLookCam.Priority;
    }

    public void EnterKiosk(Transform target, System.Action onEntered = null)
    {
        if (hudRoot) hudRoot.SetActive(false);

        // Lock movement
        if (playerMovementScript) playerMovementScript.enabled = false;

        
        if (playerInput)
        {
            
            playerInput.enabled = false;
        }

        
        if (kioskCam)
        {
            kioskCam.Follow = target;
            kioskCam.LookAt = target;
            kioskCam.Priority = 50;
            kioskCam.gameObject.SetActive(true);
        }
        if (freeLookCam)
        {
            freeLookCam.Priority = 0;
        }

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        onEntered?.Invoke();
    }

    public void ExitKiosk()
    {
        if (hudRoot) hudRoot.SetActive(true);

        if (playerMovementScript) playerMovementScript.enabled = true;

        if (playerInput)
        {
            playerInput.enabled = true;
        }

        if (kioskCam)
        {
            kioskCam.Priority = kioskDefaultPriority;
            kioskCam.gameObject.SetActive(false); 
        }
        if (freeLookCam)
        {
            freeLookCam.Priority = freeLookDefaultPriority;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}