using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Base Weapon")]
    public Transform firePoint;
    public float dps;
    public int damage;
    public LayerMask layerMask;

    public bool canAim;
    public FireRaycast aimPoint;

    [HideInInspector] public GameObject player;
    [HideInInspector] public float damageMult => PlayerManager.instance.stats.skills[1].currentMult;
    [HideInInspector] public float attackSpeedMult = 1; //=> dps * PlayerManager.instance.stats.skills[4].currentMult; 

    public PlayerInputActions.PlayerActions playerInputs;

    public abstract void Fire();

    public void Start()
    {
        player = PlayerManager.instance.player;
        playerInputs = PlayerManager.instance.playerInputs;
        playerInputs.Enable();
        if (player == null)
        {
            Debug.Log(gameObject.name + " is missing player");
            return;
        }
    }

    public void Update()
    {
        if (canAim)
        {
            if (aimPoint.hit)
            {
                firePoint.LookAt(aimPoint.hitInfo.point);
            }
            else
            {
                firePoint.LookAt(aimPoint.transform.position + aimPoint.transform.forward * aimPoint.length);
            }
        }

    }
    
    void OnDestroy()
    {
        playerInputs.Disable();
    }

    public void DestroyWeapon()
    {
        Destroy(gameObject);
    }
}
