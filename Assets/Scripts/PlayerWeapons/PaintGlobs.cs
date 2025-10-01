using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public int bulletDamage = 10;
    void Update()
    {
        bulletDamage = PlayerManager.instance.player.GetComponent<PlayerWeapon>().damage;
    }
    public int damage = 10;
    
    public float launchForce = 15f;
    private Rigidbody rb;


    void Awake()
    {
        // Get the Rigidbody component attached to this GameObject
        rb = GetComponent<Rigidbody>();
    }
    void Start()
    {
        rb.AddForce(transform.forward * launchForce, ForceMode.Impulse);
    }
    new void OnCollisionStay(Collision other)
    {
        Paint(other);
        if (other.transform.tag == "Enemy")
        {
            other.gameObject.GetComponent<Health>().Damage(bulletDamage);
        }
        Destroy(gameObject);

    }
}
