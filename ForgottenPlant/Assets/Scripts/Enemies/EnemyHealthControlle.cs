using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 3;

    [Header("UI")]
    [SerializeField] private Image healthBarFill;

    [Header("Death")]
    [SerializeField] private GameObject deathVfxPrefab;
    [SerializeField] private float destroyDelay = 0f;

    [Header("Debug")]
    [SerializeField] private bool debugLogHealth = false;

    private int currentHealth;
    private bool isDead = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        if (damage <= 0)
            return;

        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        if (debugLogHealth)
        {
            Debug.Log($"{name}: took {damage} damage -> {currentHealth}/{maxHealth}");
        }

        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        if (healthBarFill == null)
            return;

        float fillValue = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        healthBarFill.fillAmount = fillValue;
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;

        if (debugLogHealth)
        {
            Debug.Log($"{name}: died.");
        }

        if (deathVfxPrefab != null)
        {
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, destroyDelay);
    }
}
