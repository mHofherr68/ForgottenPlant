using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    // Maximum amount of health the player starts with.
    [SerializeField] private int maxHealth = 100;

    [Header("UI")]
    // UI image used as the health bar fill element.
    [SerializeField] private Image healthFillImage;

    [Header("References")]
    // Reference to the game controller used to trigger the game over flow.
    [SerializeField] private GameController gameController;

    [Header("Runtime Debug")]
    // Current health value shown in the Inspector during runtime.
    [SerializeField] private int currentHealth;

    [Header("Debug")]
    // Enables debug logs for incoming damage and death.
    [SerializeField] private bool debugLogDamage = false;

    // Public read-only access to the current health value.
    public int CurrentHealth => currentHealth;

    // Public read-only access to the maximum health value.
    public int MaxHealth => maxHealth;

    // Returns true when the player's health has reached zero.
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        // Auto-find the game controller if no reference was assigned manually.
        if (gameController == null)
            gameController = FindFirstObjectByType<GameController>();

        // Initialize the player with full health.
        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        // Ignore incoming damage if the player is already dead.
        if (IsDead)
            return;

        // Ignore invalid or non-positive damage values.
        if (damageAmount <= 0)
            return;

        // Apply damage to the player's current health.
        currentHealth -= damageAmount;

        // Clamp health so it never goes below zero.
        if (currentHealth < 0)
            currentHealth = 0;

        // Refresh the health UI after taking damage.
        UpdateHealthUI();

        if (debugLogDamage)
        {
            Debug.Log($"{name}: Took {damageAmount} damage. Health = {currentHealth}/{maxHealth}");
        }

        // If health reaches zero, trigger the death/game over logic.
        if (currentHealth == 0)
        {
            if (debugLogDamage)
            {
                Debug.Log($"{name}: Player is dead.");
            }

            if (gameController != null)
            {
                gameController.OnPlayerDeath();
            }
        }
    }

    private void UpdateHealthUI()
    {
        // Stop if no health bar image is assigned.
        if (healthFillImage == null)
            return;

        // Convert current health into a normalized fill amount.
        float fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        healthFillImage.fillAmount = fillAmount;
    }
}