using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public int damage;
    new void OnCollisionStay(Collision other)
    {
        Paint(other);
        if (other.transform.tag == "Enemy")
        {
            other.gameObject.GetComponent<Health>().Damage(damage);
        }
        Destroy(gameObject);
    }
}
