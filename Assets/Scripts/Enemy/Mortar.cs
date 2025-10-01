using UnityEngine;

public class Mortar : MonoBehaviour
{
    public GameObject hitZone;
    public int damage;
    public Vector3 offSet;
    public Vector2 moneyToAdd;
    private GameObject m_player;

    void Start()
    {
        m_player = PlayerManager.instance.player;
    }

    void Update()
    {

    }

    public void Shoot()
    {
        Debug.Log("shot");
        GameObject temp = Instantiate(hitZone, m_player.transform.position + offSet, transform.rotation);
        temp.GetComponent<DamageOrb>().damage = damage;
    }
    public void Dead()
    {
        PlayerManager.instance.wallet.Add((int)Random.Range(moneyToAdd.x, moneyToAdd.y));
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        EnemyManager.instance.RemoveEnemy();
    }
}
