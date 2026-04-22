using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthController : MonoBehaviour
{
    [Header("References")]
    // Optional reference to the enemy brain used for reaction logic on hit.
    [SerializeField] private EnemyBrain enemyBrain;

    [Header("Health Settings")]
    // Maximum health value the enemy starts with.
    [SerializeField] private int maxHealth = 3;

    [Header("UI")]
    // UI image used as the enemy health bar fill.
    [SerializeField] private Image healthBarFill;

    [Header("Death")]
    // Optional visual effect spawned when the enemy dies.
    [SerializeField] private GameObject deathVfxPrefab;

    // Delay before the enemy GameObject is destroyed after death.
    [SerializeField] private float destroyDelay = 0f;

    [Header("Debug")]
    // Enables debug logs for damage and death events.
    [SerializeField] private bool debugLogHealth = false;

    // Current health value at runtime.
    private int currentHealth;

    // Prevents repeated death handling.
    private bool isDead = false;

    // Public read-only access to current health.
    public int CurrentHealth => currentHealth;

    // Public read-only access to maximum health.
    public int MaxHealth => maxHealth;

    // Public read-only access to death state.
    public bool IsDead => isDead;

    private void Awake()
    {
        // Auto-assign the enemy brain if no reference was set manually.
        if (enemyBrain == null)
            enemyBrain = GetComponent<EnemyBrain>();
    }

    private void Start()
    {
        // Initialize health at the start of the scene.
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damage)
    {
        // Ignore incoming damage if the enemy is already dead.
        if (isDead)
            return;

        // Ignore invalid or non-positive damage values.
        if (damage <= 0)
            return;

        // Inform the enemy brain that this enemy was hit directly.
        if (enemyBrain != null)
        {
            enemyBrain.OnDirectHit(transform.position);
        }

        // Apply damage and clamp health to zero.
        currentHealth -= damage;

        if (currentHealth < 0)
            currentHealth = 0;

        if (debugLogHealth)
        {
            Debug.Log($"{name}: took {damage} damage -> {currentHealth}/{maxHealth}");
        }

        // Refresh the health bar after taking damage.
        UpdateHealthBar();

        // Trigger death once health reaches zero.
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthBar()
    {
        // Skip UI update if no health bar image is assigned.
        if (healthBarFill == null)
            return;

        // Convert current health into a normalized fill value.
        float fillValue = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        healthBarFill.fillAmount = fillValue;
    }

    private void Die()
    {
        // Prevent multiple death calls.
        if (isDead)
            return;

        isDead = true;

        if (debugLogHealth)
        {
            Debug.Log($"{name}: died.");
        }

        // Spawn death VFX if assigned.
        if (deathVfxPrefab != null)
        {
            Instantiate(deathVfxPrefab, transform.position, Quaternion.identity);
        }

        // Destroy the enemy object after the configured delay.
        Destroy(gameObject, destroyDelay);
    }
}