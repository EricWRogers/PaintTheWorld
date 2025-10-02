using UnityEngine;

public abstract class Weapon : MonoBehaviour
{
    [Header("Base Weapon")]
    public Transform firePoint;
    public float dps;
    public int damage;
    public LayerMask layerMask;
    public Color paintColor;

    [HideInInspector] public GameObject player;
    [HideInInspector] public float damageMult => PlayerManager.instance.stats.skills[1].currentMult;
    [HideInInspector] public float damageAccumulator = 0f;

    public abstract void Fire();

    public void Start()
    {
        player = PlayerManager.instance.player;
    }

    public void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Fire();
        }
    }

    public void DestroyWeapon()
    {
        Destroy(gameObject);
    }
}
