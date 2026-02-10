using SuperPupSystems.Helper;
using UnityEngine;

public abstract class  Enemy : MonoBehaviour
{
    public GameObject player;

    public GameObject bulletPrefab;
    public int baseDamage = 20;
    public int startingHealth = 100;
    public float attackSpeed = 4;
    public float rotationSpeed = 10;
    public float attackRange;
    public Transform firePoint;
    protected Rigidbody p_rb;
    [Header("Hurt FX")]
    public GameObject damageText;
    public Transform damageTextSpawn;
    public MeshRenderer modelMeshRenderer;
    public Color hurtColor;
    public float flashTime;
    private float m_flashTimer;

    private int m_tempHealth;
    private Health m_health;

    void OnEnable()
    {
        m_health = GetComponent<Health>();
        m_health.currentHealth = m_health.maxHealth;
        m_tempHealth = m_health.currentHealth;
        m_health.hurt.AddListener(SpawnDamageText);
    }
    public void Start()
    {
    }

    public void Update()
    {
        if(player == null)
        {
            player = PlayerManager.instance.player;
            return;
        }
        m_flashTimer -= Time.deltaTime;
        if(m_flashTimer <= 0)
        {
            modelMeshRenderer.materials[1].color = Color.clear;
        }
    }

    public abstract void Attack();

    public void SpawnDamageText()
    {
        int damageAmount = m_tempHealth - m_health.currentHealth;
        if (damageAmount > 0)
        {
            FloatingDamageNumbers damageNumbers = Instantiate(damageText, damageTextSpawn.position, Quaternion.identity).GetComponent<FloatingDamageNumbers>();
            damageNumbers.SetDamageText(damageAmount);
            m_tempHealth = m_health.currentHealth;
        }

    }
    public void FlashRed()
    {
        modelMeshRenderer.materials[1].color = hurtColor;
        m_flashTimer = flashTime;
    }
}
