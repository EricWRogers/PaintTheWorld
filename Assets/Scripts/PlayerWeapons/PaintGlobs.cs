using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public int bulletDamage = 10;
    void Update()
    {
        bulletDamage = PlayerManager.instance.player.GetComponent<PlayerWeapon>().damage;
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
