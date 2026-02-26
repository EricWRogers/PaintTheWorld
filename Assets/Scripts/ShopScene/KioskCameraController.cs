using System;
using UnityEngine;
using Unity.Cinemachine;

public class KioskCameraController : MonoBehaviour
{
    public static KioskCameraController I { get; private set; }

    [Header("Gameplay Camera")]
    public CinemachineVirtualCameraBase gameplayCam; 
    public int gameplayPriority = 10;

    [Header("Kiosk Priority")]
    public int kioskPriority = 50;

    [Header("Locking")]
    public MonoBehaviour playerMovementScript; 
    public GameObject hudRoot;                 

    private CinemachineVirtualCameraBase _activeKioskCam;
    private bool _inKiosk;
    private Action _onEntered;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
        ForceResetToPlayer();
    }

    void OnEnable()
    {
        ForceResetToPlayer();
    }

    public void ForceResetToPlayer()
    {
        if (gameplayCam) gameplayCam.Priority = gameplayPriority;
        if (_activeKioskCam) _activeKioskCam.Priority = 0; 
        _inKiosk = false;
    }

    public void EnterKiosk(CinemachineVirtualCameraBase kioskCam, Transform lookTarget, Action onEntered = null)
    {
        if (!kioskCam)
        {
            Debug.LogWarning("EnterKiosk called with null kioskCam.");
            return;
        }

        _activeKioskCam = kioskCam;
        _onEntered = onEntered;

       
        if (lookTarget)
        {
            kioskCam.LookAt = lookTarget;
            kioskCam.Follow = null; 
        }

       
        if (gameplayCam) gameplayCam.Priority = gameplayPriority;
        _activeKioskCam.Priority = kioskPriority;

        // Lock player movement
        if (playerMovementScript) playerMovementScript.enabled = false;

        
        if (hudRoot) hudRoot.SetActive(false);

        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _inKiosk = true;
        _onEntered?.Invoke();
    }

    public void ExitKiosk()
    {
        if (!_inKiosk) return;

       
        if (_activeKioskCam) _activeKioskCam.Priority = gameplayPriority - 1; 
        if (gameplayCam) gameplayCam.Priority = kioskPriority;

        
        if (playerMovementScript) playerMovementScript.enabled = true;

        
        if (hudRoot) hudRoot.SetActive(true);

        
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _activeKioskCam = null;
        _inKiosk = false;
    }
}