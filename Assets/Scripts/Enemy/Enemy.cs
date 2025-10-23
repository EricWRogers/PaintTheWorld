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
    public GameObject damageText;
    public Transform damageTextSpawn;

    private int m_tempHealth;
    private Health m_health;

    void OnEnable()
    {
        m_health = GetComponent<Health>();
        m_health.maxHealth = Mathf.RoundToInt(health * GameManager.instance.EnemyHealthModifier);
        m_health.currentHealth = m_health.maxHealth;
        currentDamage = baseDamage * GameManager.instance.EnemyDamageModifier;
        m_tempHealth = m_health.currentHealth;
        m_health.hurt.AddListener(SpawnDamageText);
    }
    public void Start()
    {
        player = PlayerManager.instance.player;
    }

    public abstract void Attack();

    public void SpawnDamageText()
    {
        int damageAmount = m_tempHealth - m_health.currentHealth;
        if(damageAmount > 0)
        {
            FloatingDamageNumbers damageNumbers = Instantiate(damageText, damageTextSpawn.position, Quaternion.identity).GetComponent<FloatingDamageNumbers>();
            damageNumbers.SetDamageText(damageAmount);
            m_tempHealth = m_health.currentHealth;
        }
        
    }

    public void Dead()
    {
        EnemyManager.instance.EnemyKilled();
        GameObject temp = Instantiate(coin, transform.position + coinOffset, transform.rotation);
        temp.GetComponent<TempCoinPickup>().amount = (int)(Random.Range(moneyToAdd.x, moneyToAdd.y) * GameManager.instance.CoinGainModifier);
        gameObject.SetActive(false);
    }
}
