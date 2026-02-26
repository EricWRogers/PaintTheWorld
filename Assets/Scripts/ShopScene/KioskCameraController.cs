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

    [Header("HUD")]
    public GameObject hudRoot;

    [Header("Disable While In Kiosk (IMPORTANT: do NOT put the Player root here)")]
    public MonoBehaviour[] disableTheseScripts;
    public GameObject[] disableTheseObjects;

    private CinemachineVirtualCameraBase _activeKioskCam;
    private bool _inKiosk;
    private Action _onEntered;

    private KioskLock lockComp;

    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this;
    }

    void Start()
    {
        
        ForceResetToGameplay();
    }

    void CacheLock()
    {
        if (lockComp) return;
        var pm = PlayerManager.instance;
        if (pm && pm.player) lockComp = pm.player.GetComponent<KioskLock>();
    }

    public void ForceResetToGameplay()
    {
        
        if (gameplayCam) gameplayCam.Priority = gameplayPriority;
        if (_activeKioskCam) _activeKioskCam.Priority = gameplayPriority - 1;

        _activeKioskCam = null;
        _inKiosk = false;

        if (hudRoot) hudRoot.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        CacheLock();
        if (lockComp) lockComp.SetLocked(false);
    }

    public void EnterKiosk(CinemachineVirtualCameraBase kioskCam, Transform lookTarget, Action onEntered = null)
    {
        if (!kioskCam)
        {
            Debug.LogWarning("EnterKiosk called with null kioskCam.");
            return;
        }

        CacheLock();
        if (lockComp) lockComp.SetLocked(true);

       
        if (disableTheseScripts != null)
            foreach (var b in disableTheseScripts)
                if (b) b.enabled = false;

        if (disableTheseObjects != null)
            foreach (var go in disableTheseObjects)
                if (go) go.SetActive(false);

        _activeKioskCam = kioskCam;
        _onEntered = onEntered;

        if (lookTarget)
        {
            kioskCam.LookAt = lookTarget;
            kioskCam.Follow = null;
        }

       
        if (gameplayCam) gameplayCam.Priority = gameplayPriority;
        _activeKioskCam.Priority = kioskPriority;

        if (hudRoot) hudRoot.SetActive(false);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        _inKiosk = true;
        _onEntered?.Invoke();
    }

    public void ExitKiosk()
    {
        if (!_inKiosk) return;

        CacheLock();
        if (lockComp) lockComp.SetLocked(false);

       
        if (disableTheseScripts != null)
            foreach (var b in disableTheseScripts)
                if (b) b.enabled = true;

        if (disableTheseObjects != null)
            foreach (var go in disableTheseObjects)
                if (go) go.SetActive(true);

        
        if (_activeKioskCam) _activeKioskCam.Priority = gameplayPriority - 1;
        if (gameplayCam) gameplayCam.Priority = kioskPriority;

        if (hudRoot) hudRoot.SetActive(true);

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        _activeKioskCam = null;
        _inKiosk = false;
    }
}