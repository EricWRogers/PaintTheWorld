using SuperPupSystems.Helper;
using UnityEngine;

public class Mortar : Enemy
{
    public Vector3 offSet;

    new void Start()
    {
        base.Start();
        GetComponent<Timer>().StartTimer(attackSpeed, true);
    }

    public override void Attack()
    {
        Debug.Log("shot");
        GameObject temp = Instantiate(bulletPrefab, player.transform.position + offSet, transform.rotation);
        temp.GetComponent<DamageOrb>().damage = baseDamage;
    }

}
