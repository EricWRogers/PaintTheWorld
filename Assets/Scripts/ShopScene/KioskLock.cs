using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class KioskLock : MonoBehaviour
{
    [Header("What to disable while kiosk is open")]
    public MonoBehaviour[] disableBehaviours;

#if ENABLE_INPUT_SYSTEM
    public PlayerInput playerInput;
#endif

    [Header("Optional")]
    public Rigidbody rb; 
    public bool zeroVelocityOnLock = true;

    bool locked;

    void Awake()
    {
        if (!rb) rb = GetComponent<Rigidbody>();
#if ENABLE_INPUT_SYSTEM
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
#endif
    }

    public void SetLocked(bool on)
    {
        locked = on;

       
        if (disableBehaviours != null)
        {
            foreach (var b in disableBehaviours)
                if (b) b.enabled = !on;
        }

#if ENABLE_INPUT_SYSTEM
        
        if (playerInput) playerInput.enabled = !on;
#endif

       
        if (rb && zeroVelocityOnLock)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

       
        Cursor.visible = on;
        Cursor.lockState = on ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public bool IsLocked => locked;
}