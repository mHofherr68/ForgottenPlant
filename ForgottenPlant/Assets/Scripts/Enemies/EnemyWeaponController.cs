using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    [Header("References")]
    // Transform from which projectiles are spawned.
    [SerializeField] private Transform firePoint;

    // Projectile prefab that will be instantiated when firing.
    [SerializeField] private GameObject projectilePrefab;

    // Current target the enemy should aim at.
    [SerializeField] private Transform target;

    [Header("Aim Offset")]
    // Vertical / positional offset applied to the target position while aiming.
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Audio")]
    // Sound clip played when the enemy fires.
    [SerializeField] private AudioClip shootClip;

    [Header("Shoot Cooldown By Difficulty")]
    // Time between shots on easy difficulty.
    [SerializeField] private float shotCooldownEasy = 1.0f;

    // Time between shots on medium difficulty.
    [SerializeField] private float shotCooldownMedium = 0.8f;

    // Time between shots on hard difficulty.
    [SerializeField] private float shotCooldownHard = 0.6f;

    [Header("Debug")]
    // Enables debug logs when a shot is fired.
    [SerializeField] private bool debugLogShoot = false;

    // Draws the shot ray in the Scene view for debugging.
    [SerializeField] private bool debugDrawShotRay = false;

    // Time after which the next shot is allowed.
    private float nextShotTime = 0f;

    // Cached AudioSource used for shot playback.
    private AudioSource audioSource;

    private void Awake()
    {
        // Try to auto-find a child transform named "FirePoint" if no fire point is assigned.
        if (firePoint == null)
        {
            Transform foundFirePoint = transform.Find("FirePoint");
            if (foundFirePoint != null)
                firePoint = foundFirePoint;
        }

        // Cache the AudioSource on the same GameObject.
        audioSource = GetComponent<AudioSource>();
    }

    public void SetTarget(Transform newTarget)
    {
        // Assign a new target for aiming and firing.
        target = newTarget;
    }

    public bool CanFire()
    {
        // Returns true if the current cooldown has expired.
        return Time.time >= nextShotTime;
    }

    public bool TryFire()
    {
        // Prevent firing while the game is paused.
        if (Time.timeScale == 0f)
            return false;

        // Prevent firing while still on cooldown.
        if (Time.time < nextShotTime)
            return false;

        // Validate required references before firing.
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: No projectile prefab assigned.");
            return false;
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: No firePoint assigned.");
            return false;
        }

        if (target == null)
        {
            Debug.LogWarning($"{name}: No target assigned.");
            return false;
        }

        // Build the aimed target position using the configured offset.
        Vector3 aimPoint = target.position + targetOffset;
        Vector3 shotDirection = (aimPoint - firePoint.position).normalized;

        // Fallback to the fire point forward direction if the calculated direction is invalid.
        if (shotDirection.sqrMagnitude < 0.0001f)
            shotDirection = firePoint.forward;

        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

        // Spawn the projectile at the fire point.
        GameObject projectileInstance = Instantiate(
            projectilePrefab,
            firePoint.position,
            shotRotation
        );

        // Initialize the projectile and pass the root object as the owner.
        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
        if (projectile != null)
        {
            projectile.Initialize(transform.root.gameObject, Vector3.zero);
        }

        // Play firing audio if available.
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        // Set the next allowed fire time based on the current difficulty.
        nextShotTime = Time.time + GetShotCooldownByDifficulty();

        if (debugLogShoot)
        {
            Debug.Log($"{name}: Enemy shot fired toward {target.name}");
        }

        // Draw the shot direction for debugging.
        if (debugDrawShotRay)
        {
            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
        }

        return true;
    }

    private float GetShotCooldownByDifficulty()
    {
        // Read the current difficulty from the persistent settings system.
        int difficultyIndex = 0;

        if (GameSettingsManager.Instance != null && GameSettingsManager.Instance.CurrentSettings != null)
        {
            difficultyIndex = GameSettingsManager.Instance.CurrentSettings.difficultyIndex;
        }

        // Return the configured cooldown value for the current difficulty.
        switch (difficultyIndex)
        {
            case 0:
                return shotCooldownEasy;

            case 1:
                return shotCooldownMedium;

            case 2:
                return shotCooldownHard;

            default:
                return shotCooldownEasy;
        }
    }
}