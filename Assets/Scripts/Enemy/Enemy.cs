using SuperPupSystems.Helper;
using UnityEngine;

public abstract class  Enemy : MonoBehaviour
{
    public GameObject player;

    public GameObject bulletPrefab;
    public int baseDamage = 20;
    private float currentDamage;
    public int health = 100;
    public float attackSpeed = 4;
    public float rotationSpeed = 10;
    public float attackRange;
    public Vector2 moneyToAdd;
    public Transform firePoint;
    protected Rigidbody p_rb;
    public GameObject coin;
    public Vector3 coinOffset;

    void OnEnable()
    {
        GetComponent<Health>().maxHealth = Mathf.RoundToInt(health * GameManager.instance.EnemyHealthModifier);
        GetComponent<Health>().currentHealth = GetComponent<Health>().maxHealth;
        currentDamage = baseDamage * GameManager.instance.EnemyDamageModifier;
    }
    public void Start()
    {
        player = PlayerManager.instance.player;
    }

    public abstract void Attack();
    public void Dead()
    {
        EnemyManager.instance.EnemyKilled();
        GameObject temp = Instantiate(coin, transform.position + coinOffset, transform.rotation);
        temp.GetComponent<TempCoinPickup>().amount = (int)(Random.Range(moneyToAdd.x, moneyToAdd.y) * GameManager.instance.CoinGainModifier);
        gameObject.SetActive(false);
    }
}
