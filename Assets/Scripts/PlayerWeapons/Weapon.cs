using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Base Weapon")]
    public Transform firePoint;
    public float dps;
    public int damage;
    public LayerMask layerMask;

    [HideInInspector] public GameObject player;
    [HideInInspector] public float damageMult => PlayerManager.instance.stats.skills[1].currentMult;
    [HideInInspector] public float attackSpeedMult = 1; //=> dps * PlayerManager.instance.stats.skills[4].currentMult; 

    public abstract void Fire();

    public void Start()
    {
        player = PlayerManager.instance.player;
        if (player == null)
        {
            Debug.Log(gameObject.name + " is missing player");
            return;
        }
    }

    public void DestroyWeapon()
    {
        Destroy(gameObject);
    }
}
