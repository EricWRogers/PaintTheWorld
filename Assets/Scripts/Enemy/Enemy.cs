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

    public void Start()
    {
        player = PlayerManager.instance.player;
        GetComponent<Health>().maxHealth = health;
        currentDamage = baseDamage * GameManager.instance.EnemyDamageModifier;
        
    }

    public abstract void Attack();
        public void Dead()
    {
        PlayerManager.instance.wallet.Add((int)(Random.Range(moneyToAdd.x, moneyToAdd.y) * GameManager.instance.CoinGainModifier));
        Destroy(gameObject);
    }

    public void OnDestroy()
    {
        EnemyManager.instance.RemoveEnemy();
    }
}
