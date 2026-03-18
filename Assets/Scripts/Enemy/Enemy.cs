using SuperPupSystems.Helper;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(Enemy), true)]
[CanEditMultipleObjects]
public class EnemyEditor : Editor
{
    public override void OnInspectorGUI()
{
    DrawDefaultInspector();

    if (GUILayout.Button("Kill"))
    {
        foreach (Object obj in targets)
        {
            Enemy enemy = (Enemy)obj;
            enemy.Kill();
        }
    }
}
}
#endif

public abstract class  Enemy : MonoBehaviour
{

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

    private int m_tempHealth;
    private Health m_health;

    public bool targetingPlayer;

    public Transform target;
    public Animator anim;

    public float timerToSwitchTargets;
    private float m_switchTimer;


    void OnEnable()
    {
        m_health = GetComponent<Health>();
        m_health.currentHealth = m_health.maxHealth;
        m_tempHealth = m_health.currentHealth;
        m_health.hurt.AddListener(SpawnDamageText);
    }
    public void Start()
    {
        if (targetingPlayer)
        {
            target = PlayerManager.instance.player.transform;
        }
        else
        {
            PaintingObj obj = GameManager.instance.objectives[Random.Range(0, GameManager.instance.GetObjs().Count)];
            target = obj.transform;
            obj.currentEnemiesTarget++;
        }
    }

    public void Update()
    {
        if(target == null)
        {
            PaintingObj obj = GameManager.instance.objectives[Random.Range(0, GameManager.instance.GetObjs().Count)];
            target = obj.transform;
            obj.currentEnemiesTarget++;
            return;
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
    public void Hurt()
    {
        anim.SetTrigger("Hit");
    }

    public void Kill()
    {
        m_health.Kill();
    }
}
