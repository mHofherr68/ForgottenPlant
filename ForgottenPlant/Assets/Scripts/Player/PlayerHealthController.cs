//using UnityEngine;

//public class PlayerHealthController : MonoBehaviour
//{
//    [Header("Health Settings")]
//    [SerializeField] private int maxHealth = 100;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogDamage = false;

//    private int currentHealth;

//    public int CurrentHealth => currentHealth;
//    public int MaxHealth => maxHealth;
//    public bool IsDead => currentHealth <= 0;

//    private void Awake()
//    {
//        currentHealth = maxHealth;
//    }

//    public void TakeDamage(int damageAmount)
//    {
//        if (IsDead)
//            return;

//        if (damageAmount <= 0)
//            return;

//        currentHealth -= damageAmount;

//        if (currentHealth < 0)
//            currentHealth = 0;

//        if (debugLogDamage)
//        {
//            Debug.Log($"{name}: Took {damageAmount} damage. Health = {currentHealth}/{maxHealth}");
//        }

//        if (currentHealth == 0)
//        {
//            if (debugLogDamage)
//            {
//                Debug.Log($"{name}: Player is dead.");
//            }
//        }
//    }
//}
//using UnityEngine;
//using UnityEngine.UI;

//public class PlayerHealthController : MonoBehaviour
//{
//    [Header("Health Settings")]
//    [SerializeField] private int maxHealth = 100;

//    [Header("UI")]
//    [SerializeField] private Image healthFillImage;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogDamage = false;

//    private int currentHealth;

//    public int CurrentHealth => currentHealth;
//    public int MaxHealth => maxHealth;
//    public bool IsDead => currentHealth <= 0;

//    private void Awake()
//    {
//        currentHealth = maxHealth;
//        UpdateHealthUI();
//    }

//    public void TakeDamage(int damageAmount)
//    {
//        if (IsDead)
//            return;

//        if (damageAmount <= 0)
//            return;

//        currentHealth -= damageAmount;

//        if (currentHealth < 0)
//            currentHealth = 0;

//        UpdateHealthUI();

//        if (debugLogDamage)
//        {
//            Debug.Log($"{name}: Took {damageAmount} damage. Health = {currentHealth}/{maxHealth}");
//        }

//        if (currentHealth == 0)
//        {
//            if (debugLogDamage)
//            {
//                Debug.Log($"{name}: Player is dead.");
//            }
//        }
//    }

//    private void UpdateHealthUI()
//    {
//        if (healthFillImage == null)
//            return;

//        float fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
//        healthFillImage.fillAmount = fillAmount;
//    }
//}
//using UnityEngine;
//using UnityEngine.UI;

//public class PlayerHealthController : MonoBehaviour
//{
//    [Header("Health Settings")]
//    [SerializeField] private int maxHealth = 100;

//    [Header("UI")]
//    [SerializeField] private Image healthFillImage;

//    [Header("Runtime Debug")]
//    [SerializeField] private int currentHealth;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogDamage = false;

//    public int CurrentHealth => currentHealth;
//    public int MaxHealth => maxHealth;
//    public bool IsDead => currentHealth <= 0;

//    private void Awake()
//    {
//        currentHealth = maxHealth;
//        UpdateHealthUI();
//    }

//    public void TakeDamage(int damageAmount)
//    {
//        if (IsDead)
//            return;

//        if (damageAmount <= 0)
//            return;

//        currentHealth -= damageAmount;

//        if (currentHealth < 0)
//            currentHealth = 0;

//        UpdateHealthUI();

//        if (debugLogDamage)
//        {
//            Debug.Log($"{name}: Took {damageAmount} damage. Health = {currentHealth}/{maxHealth}");
//        }

//        if (currentHealth == 0)
//        {
//            if (debugLogDamage)
//            {
//                Debug.Log($"{name}: Player is dead.");
//            }
//        }
//    }

//    private void UpdateHealthUI()
//    {
//        if (healthFillImage == null)
//            return;

//        float fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
//        healthFillImage.fillAmount = fillAmount;
//    }
//}
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthController : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private int maxHealth = 100;

    [Header("UI")]
    [SerializeField] private Image healthFillImage;

    [Header("References")]
    [SerializeField] private GameController gameController;

    [Header("Runtime Debug")]
    [SerializeField] private int currentHealth;

    [Header("Debug")]
    [SerializeField] private bool debugLogDamage = false;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => currentHealth <= 0;

    private void Awake()
    {
        if (gameController == null)
            gameController = FindFirstObjectByType<GameController>();

        currentHealth = maxHealth;
        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        if (IsDead)
            return;

        if (damageAmount <= 0)
            return;

        currentHealth -= damageAmount;

        if (currentHealth < 0)
            currentHealth = 0;

        UpdateHealthUI();

        if (debugLogDamage)
        {
            Debug.Log($"{name}: Took {damageAmount} damage. Health = {currentHealth}/{maxHealth}");
        }

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
        if (healthFillImage == null)
            return;

        float fillAmount = maxHealth > 0 ? (float)currentHealth / maxHealth : 0f;
        healthFillImage.fillAmount = fillAmount;
    }
}