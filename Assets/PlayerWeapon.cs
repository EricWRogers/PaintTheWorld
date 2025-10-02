using Unity.VisualScripting;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    public GameObject weaponSlot;
    public float paintRadius;
    public int damage;


    public void ChangeWeapon(GameObject _gunPrefab)
    {
        if (weaponSlot.transform.childCount > 0)
        {
            Weapon temp = weaponSlot.GetComponentInChildren<Weapon>();
            temp.DestroyWeapon();
            Instantiate(_gunPrefab, weaponSlot.transform);
        }
    }
}
