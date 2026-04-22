using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    // Transform from which projectiles are spawned.
    [SerializeField] private Transform firePoint;

    // Projectile prefab instantiated when the weapon fires.
    [SerializeField] private GameObject projectilePrefab;

    // Camera used for aiming and target raycasts.
    [SerializeField] private Camera playerCamera;

    [Header("Audio")]
    // Sound played when the weapon fires successfully.
    [SerializeField] private AudioClip shootClip;

    // Sound played when the weapon jams.
    [SerializeField] private AudioClip jamClip;

    [Header("Shoot Settings")]
    // Enables full-auto fire mode if true, otherwise the weapon fires semi-automatically.
    [SerializeField] private bool fullAuto = false;

    // Rate of fire used in full-auto mode.
    [SerializeField] private float fireRate = 10f;

    // Cooldown between shots in semi-auto mode.
    [SerializeField] private float shotCooldown = 0.6f;

    // Maximum distance used for aim raycasts.
    [SerializeField] private float maxAimDistance = 100f;

    // Layer mask used for aiming raycasts.
    [SerializeField] private LayerMask aimMask = ~0;

    [Header("Jam Settings")]
    // Enables simulated random weapon jams.
    [SerializeField] private bool simulateRandomJam = false;

    // Chance that the weapon jams when trying to shoot.
    [SerializeField, Range(0f, 1f)] private float jamChance = 0.1f;

    [Header("Zoom Settings")]
    // Enables weapon zoom functionality.
    [SerializeField] private bool enableZoom = false;

    // Target field of view while zooming.
    [SerializeField] private float zoomFov = 25f;

    // Speed used to smoothly interpolate the camera field of view.
    [SerializeField] private float zoomSmoothSpeed = 10f;

    // Optional UI image shown while zooming.
    [SerializeField] private Image zoomUiImage;

    [Header("Debug")]
    // Enables debug logs for firing and jam events.
    [SerializeField] private bool debugLogShoot = false;

    // Draws the shot direction in the Scene view for debugging.
    [SerializeField] private bool debugDrawShotRay = false;

    // Timestamp when the next shot is allowed.
    private float nextShotTime = 0f;

    // Cached AudioSource used for weapon sounds.
    private AudioSource audioSource;

    // Cached player controller used for inherited projectile velocity.
    private PlayerController playerController;

    // Default camera field of view used when not zooming.
    private float defaultFov;

    private void Awake()
    {
        // Try to auto-find the fire point if it was not assigned manually.
        if (firePoint == null)
        {
            Transform foundFirePoint = transform.Find("PM_Firepoint");
            if (foundFirePoint != null)
                firePoint = foundFirePoint;
        }

        // Use the main camera if no player camera was assigned manually.
        if (playerCamera == null)
            playerCamera = Camera.main;

        // Cache the AudioSource on this GameObject.
        audioSource = GetComponent<AudioSource>();

        // Try to find the player controller on the parent hierarchy.
        playerController = GetComponentInParent<PlayerController>();

        if (playerController == null && transform.root != null)
            playerController = transform.root.GetComponent<PlayerController>();

        // Cache the default field of view for zoom reset.
        if (playerCamera != null)
            defaultFov = playerCamera.fieldOfView;

        // Ensure zoom UI is hidden at startup.
        if (zoomUiImage != null)
            zoomUiImage.enabled = false;
    }

    private void Update()
    {
        // Block gameplay input while the game is paused.
        // UI and mouse input for pause menus remain available.
        if (Time.timeScale == 0f)
            return;

        if (Mouse.current == null)
            return;

        // Handle zoom every frame before firing logic.
        HandleZoom();

        if (fullAuto)
        {
            HandleFullAutoFire();
        }
        else
        {
            // Semi-auto weapons only fire on the press frame.
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryShoot(shotCooldown);
            }
        }
    }

    private void HandleZoom()
    {
        // If zoom is disabled, hide the zoom UI and return.
        if (!enableZoom)
        {
            if (zoomUiImage != null)
                zoomUiImage.enabled = false;

            return;
        }

        if (playerCamera == null)
            return;

        if (Mouse.current == null)
            return;

        // Right mouse button controls zoom state.
        bool isZooming = Mouse.current.rightButton.isPressed;
        float targetFov = isZooming ? zoomFov : defaultFov;

        // Smoothly interpolate the camera field of view.
        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFov,
            zoomSmoothSpeed * Time.deltaTime
        );

        // Show or hide the optional zoom overlay.
        if (zoomUiImage != null)
            zoomUiImage.enabled = isZooming;
    }

    private void HandleFullAutoFire()
    {
        // Full-auto only continues firing while the left mouse button is held.
        if (!Mouse.current.leftButton.isPressed)
            return;

        float autoShotInterval = fireRate > 0f ? 1f / fireRate : 0.01f;
        TryShoot(autoShotInterval);
    }

    private void TryShoot(float cooldown)
    {
        // Additional safety check in case this method is called from somewhere else later.
        if (Time.timeScale == 0f)
            return;

        // Respect current weapon cooldown.
        if (Time.time < nextShotTime)
            return;

        // Validate all required firing references.
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"{name}: No projectile prefab assigned.");
            return;
        }

        if (firePoint == null)
        {
            Debug.LogWarning($"{name}: No firePoint assigned.");
            return;
        }

        if (playerCamera == null)
        {
            Debug.LogWarning($"{name}: No player camera assigned.");
            return;
        }

        // Simulate a random weapon jam if enabled.
        if (simulateRandomJam && Random.value < jamChance)
        {
            if (audioSource != null && jamClip != null)
            {
                audioSource.PlayOneShot(jamClip);
            }

            nextShotTime = Time.time + cooldown;

            if (debugLogShoot)
            {
                Debug.Log($"{name}: Weapon jammed.");
            }

            return;
        }

        // Determine the current aim target point from the center of the screen.
        Vector3 targetPoint = GetAimTargetPoint();
        Vector3 shotDirection = (targetPoint - firePoint.position).normalized;

        // Fallback to the fire point forward direction if the aim direction is invalid.
        if (shotDirection.sqrMagnitude < 0.0001f)
            shotDirection = firePoint.forward;

        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

        // Spawn the projectile at the fire point.
        GameObject projectileInstance = Instantiate(
            projectilePrefab,
            firePoint.position,
            shotRotation
        );

        // Pass weapon owner and inherited player movement velocity to the projectile.
        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
        if (projectile != null)
        {
            Vector3 inheritedVelocity = playerController != null ? playerController.WorldMoveVelocity : Vector3.zero;
            projectile.Initialize(transform.root.gameObject, inheritedVelocity);
        }

        // Play the firing sound if available.
        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        // Notify all enemies that a gunshot was fired.
        EnemyBrain.NotifyGunshot(firePoint.position);

        // Set the next allowed fire time.
        nextShotTime = Time.time + cooldown;

        if (debugLogShoot)
        {
            Debug.Log($"{name}: Shot fired toward {targetPoint}");
        }

        // Optionally draw the shot ray for debugging.
        if (debugDrawShotRay)
        {
            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
        }
    }

    private Vector3 GetAimTargetPoint()
    {
        // Create a ray from the center of the screen.
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        // Return the hit point if something is hit inside the aim distance.
        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        // Otherwise return a point straight ahead at maximum aim distance.
        return ray.GetPoint(maxAimDistance);
    }
}