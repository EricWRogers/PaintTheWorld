using SuperPupSystems.Helper;
using UnityEngine;

public class PaintGlobs : CollisonPainter
{
    public int damage = 10;
    new void OnCollisionStay(Collision other)
    {
        Debug.Log("Hit" + other.transform.name);
        Paint(other);
        if (other.transform.tag == "Enemy")
        {
            other.gameObject.GetComponent<Health>().Damage(damage);
        }
        Destroy(gameObject);
    }
}
