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
            Destroy(weaponSlot.transform.GetChild(0));
            Instantiate(_gunPrefab, weaponSlot.transform);
        }
    }
}
