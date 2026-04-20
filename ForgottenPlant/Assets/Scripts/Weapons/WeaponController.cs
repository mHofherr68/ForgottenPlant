//using UnityEngine;
//using UnityEngine.InputSystem;

//public class WeaponController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private GameObject projectilePrefab;
//    [SerializeField] private Camera playerCamera;

//    [Header("Audio")]
//    [SerializeField] private AudioClip shootClip;

//    [Header("Shoot Settings")]
//    [SerializeField] private bool fullAuto = false;
//    [SerializeField] private float fireRate = 10f;
//    [SerializeField] private float shotCooldown = 0.6f;
//    [SerializeField] private float maxAimDistance = 100f;
//    [SerializeField] private LayerMask aimMask = ~0;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogShoot = false;
//    [SerializeField] private bool debugDrawShotRay = false;

//    private float nextShotTime = 0f;
//    private AudioSource audioSource;
//    private PlayerController playerController;

//    private void Awake()
//    {
//        if (firePoint == null)
//        {
//            Transform foundFirePoint = transform.Find("PM_Firepoint");
//            if (foundFirePoint != null)
//                firePoint = foundFirePoint;
//        }

//        if (playerCamera == null)
//            playerCamera = Camera.main;

//        audioSource = GetComponent<AudioSource>();
//        playerController = GetComponentInParent<PlayerController>();

//        if (playerController == null && transform.root != null)
//            playerController = transform.root.GetComponent<PlayerController>();
//    }

//    private void Update()
//    {
//        // Gameplay-Input blockieren, wenn das Spiel pausiert ist.
//        // UI/Maus für das Pausemenü bleiben dabei trotzdem aktiv.
//        if (Time.timeScale == 0f)
//            return;

//        if (Mouse.current == null)
//            return;

//        if (fullAuto)
//        {
//            HandleFullAutoFire();
//        }
//        else
//        {
//            if (Mouse.current.leftButton.wasPressedThisFrame)
//            {
//                TryShoot(shotCooldown);
//            }
//        }
//    }

//    private void HandleFullAutoFire()
//    {
//        if (!Mouse.current.leftButton.isPressed)
//            return;

//        float autoShotInterval = fireRate > 0f ? 1f / fireRate : 0.01f;
//        TryShoot(autoShotInterval);
//    }

//    private void TryShoot(float cooldown)
//    {
//        // Zweite Sicherheit, falls TryShoot später mal von woanders aufgerufen wird.
//        if (Time.timeScale == 0f)
//            return;

//        if (Time.time < nextShotTime)
//            return;

//        if (projectilePrefab == null)
//        {
//            Debug.LogWarning($"{name}: No projectile prefab assigned.");
//            return;
//        }

//        if (firePoint == null)
//        {
//            Debug.LogWarning($"{name}: No firePoint assigned.");
//            return;
//        }

//        if (playerCamera == null)
//        {
//            Debug.LogWarning($"{name}: No player camera assigned.");
//            return;
//        }

//        Vector3 targetPoint = GetAimTargetPoint();
//        Vector3 shotDirection = (targetPoint - firePoint.position).normalized;

//        if (shotDirection.sqrMagnitude < 0.0001f)
//            shotDirection = firePoint.forward;

//        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

//        GameObject projectileInstance = Instantiate(
//            projectilePrefab,
//            firePoint.position,
//            shotRotation
//        );

//        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
//        if (projectile != null)
//        {
//            Vector3 inheritedVelocity = playerController != null ? playerController.WorldMoveVelocity : Vector3.zero;
//            projectile.Initialize(transform.root.gameObject, inheritedVelocity);
//        }

//        if (audioSource != null && shootClip != null)
//        {
//            audioSource.PlayOneShot(shootClip);
//        }

//        EnemyBrain.NotifyGunshot(firePoint.position);

//        nextShotTime = Time.time + cooldown;

//        if (debugLogShoot)
//        {
//            Debug.Log($"{name}: Shot fired toward {targetPoint}");
//        }

//        if (debugDrawShotRay)
//        {
//            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
//        }
//    }

//    private Vector3 GetAimTargetPoint()
//    {
//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

//        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
//        {
//            return hit.point;
//        }

//        return ray.GetPoint(maxAimDistance);
//    }
//}
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class WeaponController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private GameObject projectilePrefab;
//    [SerializeField] private Camera playerCamera;

//    [Header("Audio")]
//    [SerializeField] private AudioClip shootClip;
//    [SerializeField] private AudioClip jamClip;

//    [Header("Shoot Settings")]
//    [SerializeField] private bool fullAuto = false;
//    [SerializeField] private float fireRate = 10f;
//    [SerializeField] private float shotCooldown = 0.6f;
//    [SerializeField] private float maxAimDistance = 100f;
//    [SerializeField] private LayerMask aimMask = ~0;

//    [Header("Jam Settings")]
//    [SerializeField] private bool simulateRandomJam = false;
//    [SerializeField, Range(0f, 1f)] private float jamChance = 0.1f;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogShoot = false;
//    [SerializeField] private bool debugDrawShotRay = false;

//    private float nextShotTime = 0f;
//    private AudioSource audioSource;
//    private PlayerController playerController;

//    private void Awake()
//    {
//        if (firePoint == null)
//        {
//            Transform foundFirePoint = transform.Find("PM_Firepoint");
//            if (foundFirePoint != null)
//                firePoint = foundFirePoint;
//        }

//        if (playerCamera == null)
//            playerCamera = Camera.main;

//        audioSource = GetComponent<AudioSource>();
//        playerController = GetComponentInParent<PlayerController>();

//        if (playerController == null && transform.root != null)
//            playerController = transform.root.GetComponent<PlayerController>();
//    }

//    private void Update()
//    {
//        // Gameplay-Input blockieren, wenn das Spiel pausiert ist.
//        // UI/Maus für das Pausemenü bleiben dabei trotzdem aktiv.
//        if (Time.timeScale == 0f)
//            return;

//        if (Mouse.current == null)
//            return;

//        if (fullAuto)
//        {
//            HandleFullAutoFire();
//        }
//        else
//        {
//            if (Mouse.current.leftButton.wasPressedThisFrame)
//            {
//                TryShoot(shotCooldown);
//            }
//        }
//    }

//    private void HandleFullAutoFire()
//    {
//        if (!Mouse.current.leftButton.isPressed)
//            return;

//        float autoShotInterval = fireRate > 0f ? 1f / fireRate : 0.01f;
//        TryShoot(autoShotInterval);
//    }

//    private void TryShoot(float cooldown)
//    {
//        // Zweite Sicherheit, falls TryShoot später mal von woanders aufgerufen wird.
//        if (Time.timeScale == 0f)
//            return;

//        if (Time.time < nextShotTime)
//            return;

//        if (projectilePrefab == null)
//        {
//            Debug.LogWarning($"{name}: No projectile prefab assigned.");
//            return;
//        }

//        if (firePoint == null)
//        {
//            Debug.LogWarning($"{name}: No firePoint assigned.");
//            return;
//        }

//        if (playerCamera == null)
//        {
//            Debug.LogWarning($"{name}: No player camera assigned.");
//            return;
//        }

//        if (simulateRandomJam && Random.value < jamChance)
//        {
//            if (audioSource != null && jamClip != null)
//            {
//                audioSource.PlayOneShot(jamClip);
//            }

//            nextShotTime = Time.time + cooldown;

//            if (debugLogShoot)
//            {
//                Debug.Log($"{name}: Weapon jammed.");
//            }

//            return;
//        }

//        Vector3 targetPoint = GetAimTargetPoint();
//        Vector3 shotDirection = (targetPoint - firePoint.position).normalized;

//        if (shotDirection.sqrMagnitude < 0.0001f)
//            shotDirection = firePoint.forward;

//        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

//        GameObject projectileInstance = Instantiate(
//            projectilePrefab,
//            firePoint.position,
//            shotRotation
//        );

//        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
//        if (projectile != null)
//        {
//            Vector3 inheritedVelocity = playerController != null ? playerController.WorldMoveVelocity : Vector3.zero;
//            projectile.Initialize(transform.root.gameObject, inheritedVelocity);
//        }

//        if (audioSource != null && shootClip != null)
//        {
//            audioSource.PlayOneShot(shootClip);
//        }

//        EnemyBrain.NotifyGunshot(firePoint.position);

//        nextShotTime = Time.time + cooldown;

//        if (debugLogShoot)
//        {
//            Debug.Log($"{name}: Shot fired toward {targetPoint}");
//        }

//        if (debugDrawShotRay)
//        {
//            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
//        }
//    }

//    private Vector3 GetAimTargetPoint()
//    {
//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

//        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
//        {
//            return hit.point;
//        }

//        return ray.GetPoint(maxAimDistance);
//    }
//}
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class WeaponController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private GameObject projectilePrefab;
//    [SerializeField] private Camera playerCamera;

//    [Header("Audio")]
//    [SerializeField] private AudioClip shootClip;
//    [SerializeField] private AudioClip jamClip;

//    [Header("Shoot Settings")]
//    [SerializeField] private bool fullAuto = false;
//    [SerializeField] private float fireRate = 10f;
//    [SerializeField] private float shotCooldown = 0.6f;
//    [SerializeField] private float maxAimDistance = 100f;
//    [SerializeField] private LayerMask aimMask = ~0;

//    [Header("Jam Settings")]
//    [SerializeField] private bool simulateRandomJam = false;
//    [SerializeField, Range(0f, 1f)] private float jamChance = 0.1f;

//    [Header("Zoom Settings")]
//    [SerializeField] private bool enableZoom = false;
//    [SerializeField] private float zoomFov = 25f;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogShoot = false;
//    [SerializeField] private bool debugDrawShotRay = false;

//    private float nextShotTime = 0f;
//    private AudioSource audioSource;
//    private PlayerController playerController;
//    private float defaultFov;

//    private void Awake()
//    {
//        if (firePoint == null)
//        {
//            Transform foundFirePoint = transform.Find("PM_Firepoint");
//            if (foundFirePoint != null)
//                firePoint = foundFirePoint;
//        }

//        if (playerCamera == null)
//            playerCamera = Camera.main;

//        audioSource = GetComponent<AudioSource>();
//        playerController = GetComponentInParent<PlayerController>();

//        if (playerController == null && transform.root != null)
//            playerController = transform.root.GetComponent<PlayerController>();

//        if (playerCamera != null)
//            defaultFov = playerCamera.fieldOfView;
//    }

//    private void Update()
//    {
//        // Gameplay-Input blockieren, wenn das Spiel pausiert ist.
//        // UI/Maus für das Pausemenü bleiben dabei trotzdem aktiv.
//        if (Time.timeScale == 0f)
//            return;

//        if (Mouse.current == null)
//            return;

//        HandleZoom();

//        if (fullAuto)
//        {
//            HandleFullAutoFire();
//        }
//        else
//        {
//            if (Mouse.current.leftButton.wasPressedThisFrame)
//            {
//                TryShoot(shotCooldown);
//            }
//        }
//    }

//    private void HandleZoom()
//    {
//        if (!enableZoom)
//            return;

//        if (playerCamera == null)
//            return;

//        if (Mouse.current == null)
//            return;

//        if (Mouse.current.rightButton.isPressed)
//            playerCamera.fieldOfView = zoomFov;
//        else
//            playerCamera.fieldOfView = defaultFov;
//    }

//    private void HandleFullAutoFire()
//    {
//        if (!Mouse.current.leftButton.isPressed)
//            return;

//        float autoShotInterval = fireRate > 0f ? 1f / fireRate : 0.01f;
//        TryShoot(autoShotInterval);
//    }

//    private void TryShoot(float cooldown)
//    {
//        // Zweite Sicherheit, falls TryShoot später mal von woanders aufgerufen wird.
//        if (Time.timeScale == 0f)
//            return;

//        if (Time.time < nextShotTime)
//            return;

//        if (projectilePrefab == null)
//        {
//            Debug.LogWarning($"{name}: No projectile prefab assigned.");
//            return;
//        }

//        if (firePoint == null)
//        {
//            Debug.LogWarning($"{name}: No firePoint assigned.");
//            return;
//        }

//        if (playerCamera == null)
//        {
//            Debug.LogWarning($"{name}: No player camera assigned.");
//            return;
//        }

//        if (simulateRandomJam && Random.value < jamChance)
//        {
//            if (audioSource != null && jamClip != null)
//            {
//                audioSource.PlayOneShot(jamClip);
//            }

//            nextShotTime = Time.time + cooldown;

//            if (debugLogShoot)
//            {
//                Debug.Log($"{name}: Weapon jammed.");
//            }

//            return;
//        }

//        Vector3 targetPoint = GetAimTargetPoint();
//        Vector3 shotDirection = (targetPoint - firePoint.position).normalized;

//        if (shotDirection.sqrMagnitude < 0.0001f)
//            shotDirection = firePoint.forward;

//        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

//        GameObject projectileInstance = Instantiate(
//            projectilePrefab,
//            firePoint.position,
//            shotRotation
//        );

//        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
//        if (projectile != null)
//        {
//            Vector3 inheritedVelocity = playerController != null ? playerController.WorldMoveVelocity : Vector3.zero;
//            projectile.Initialize(transform.root.gameObject, inheritedVelocity);
//        }

//        if (audioSource != null && shootClip != null)
//        {
//            audioSource.PlayOneShot(shootClip);
//        }

//        EnemyBrain.NotifyGunshot(firePoint.position);

//        nextShotTime = Time.time + cooldown;

//        if (debugLogShoot)
//        {
//            Debug.Log($"{name}: Shot fired toward {targetPoint}");
//        }

//        if (debugDrawShotRay)
//        {
//            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
//        }
//    }

//    private Vector3 GetAimTargetPoint()
//    {
//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

//        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
//        {
//            return hit.point;
//        }

//        return ray.GetPoint(maxAimDistance);
//    }
//}
//using UnityEngine;
//using UnityEngine.InputSystem;

//public class WeaponController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private GameObject projectilePrefab;
//    [SerializeField] private Camera playerCamera;

//    [Header("Audio")]
//    [SerializeField] private AudioClip shootClip;
//    [SerializeField] private AudioClip jamClip;

//    [Header("Shoot Settings")]
//    [SerializeField] private bool fullAuto = false;
//    [SerializeField] private float fireRate = 10f;
//    [SerializeField] private float shotCooldown = 0.6f;
//    [SerializeField] private float maxAimDistance = 100f;
//    [SerializeField] private LayerMask aimMask = ~0;

//    [Header("Jam Settings")]
//    [SerializeField] private bool simulateRandomJam = false;
//    [SerializeField, Range(0f, 1f)] private float jamChance = 0.1f;

//    [Header("Zoom Settings")]
//    [SerializeField] private bool enableZoom = false;
//    [SerializeField] private float zoomFov = 25f;
//    [SerializeField] private float zoomSmoothSpeed = 10f;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogShoot = false;
//    [SerializeField] private bool debugDrawShotRay = false;

//    private float nextShotTime = 0f;
//    private AudioSource audioSource;
//    private PlayerController playerController;
//    private float defaultFov;

//    private void Awake()
//    {
//        if (firePoint == null)
//        {
//            Transform foundFirePoint = transform.Find("PM_Firepoint");
//            if (foundFirePoint != null)
//                firePoint = foundFirePoint;
//        }

//        if (playerCamera == null)
//            playerCamera = Camera.main;

//        audioSource = GetComponent<AudioSource>();
//        playerController = GetComponentInParent<PlayerController>();

//        if (playerController == null && transform.root != null)
//            playerController = transform.root.GetComponent<PlayerController>();

//        if (playerCamera != null)
//            defaultFov = playerCamera.fieldOfView;
//    }

//    private void Update()
//    {
//        // Gameplay-Input blockieren, wenn das Spiel pausiert ist.
//        // UI/Maus für das Pausemenü bleiben dabei trotzdem aktiv.
//        if (Time.timeScale == 0f)
//            return;

//        if (Mouse.current == null)
//            return;

//        HandleZoom();

//        if (fullAuto)
//        {
//            HandleFullAutoFire();
//        }
//        else
//        {
//            if (Mouse.current.leftButton.wasPressedThisFrame)
//            {
//                TryShoot(shotCooldown);
//            }
//        }
//    }

//    private void HandleZoom()
//    {
//        if (!enableZoom)
//            return;

//        if (playerCamera == null)
//            return;

//        if (Mouse.current == null)
//            return;

//        float targetFov = Mouse.current.rightButton.isPressed ? zoomFov : defaultFov;
//        playerCamera.fieldOfView = Mathf.Lerp(
//            playerCamera.fieldOfView,
//            targetFov,
//            zoomSmoothSpeed * Time.deltaTime
//        );
//    }

//    private void HandleFullAutoFire()
//    {
//        if (!Mouse.current.leftButton.isPressed)
//            return;

//        float autoShotInterval = fireRate > 0f ? 1f / fireRate : 0.01f;
//        TryShoot(autoShotInterval);
//    }

//    private void TryShoot(float cooldown)
//    {
//        // Zweite Sicherheit, falls TryShoot später mal von woanders aufgerufen wird.
//        if (Time.timeScale == 0f)
//            return;

//        if (Time.time < nextShotTime)
//            return;

//        if (projectilePrefab == null)
//        {
//            Debug.LogWarning($"{name}: No projectile prefab assigned.");
//            return;
//        }

//        if (firePoint == null)
//        {
//            Debug.LogWarning($"{name}: No firePoint assigned.");
//            return;
//        }

//        if (playerCamera == null)
//        {
//            Debug.LogWarning($"{name}: No player camera assigned.");
//            return;
//        }

//        if (simulateRandomJam && Random.value < jamChance)
//        {
//            if (audioSource != null && jamClip != null)
//            {
//                audioSource.PlayOneShot(jamClip);
//            }

//            nextShotTime = Time.time + cooldown;

//            if (debugLogShoot)
//            {
//                Debug.Log($"{name}: Weapon jammed.");
//            }

//            return;
//        }

//        Vector3 targetPoint = GetAimTargetPoint();
//        Vector3 shotDirection = (targetPoint - firePoint.position).normalized;

//        if (shotDirection.sqrMagnitude < 0.0001f)
//            shotDirection = firePoint.forward;

//        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

//        GameObject projectileInstance = Instantiate(
//            projectilePrefab,
//            firePoint.position,
//            shotRotation
//        );

//        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
//        if (projectile != null)
//        {
//            Vector3 inheritedVelocity = playerController != null ? playerController.WorldMoveVelocity : Vector3.zero;
//            projectile.Initialize(transform.root.gameObject, inheritedVelocity);
//        }

//        if (audioSource != null && shootClip != null)
//        {
//            audioSource.PlayOneShot(shootClip);
//        }

//        EnemyBrain.NotifyGunshot(firePoint.position);

//        nextShotTime = Time.time + cooldown;

//        if (debugLogShoot)
//        {
//            Debug.Log($"{name}: Shot fired toward {targetPoint}");
//        }

//        if (debugDrawShotRay)
//        {
//            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
//        }
//    }

//    private Vector3 GetAimTargetPoint()
//    {
//        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

//        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
//        {
//            return hit.point;
//        }

//        return ray.GetPoint(maxAimDistance);
//    }
//}
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Camera playerCamera;

    [Header("Audio")]
    [SerializeField] private AudioClip shootClip;
    [SerializeField] private AudioClip jamClip;

    [Header("Shoot Settings")]
    [SerializeField] private bool fullAuto = false;
    [SerializeField] private float fireRate = 10f;
    [SerializeField] private float shotCooldown = 0.6f;
    [SerializeField] private float maxAimDistance = 100f;
    [SerializeField] private LayerMask aimMask = ~0;

    [Header("Jam Settings")]
    [SerializeField] private bool simulateRandomJam = false;
    [SerializeField, Range(0f, 1f)] private float jamChance = 0.1f;

    [Header("Zoom Settings")]
    [SerializeField] private bool enableZoom = false;
    [SerializeField] private float zoomFov = 25f;
    [SerializeField] private float zoomSmoothSpeed = 10f;
    [SerializeField] private Image zoomUiImage;

    [Header("Debug")]
    [SerializeField] private bool debugLogShoot = false;
    [SerializeField] private bool debugDrawShotRay = false;

    private float nextShotTime = 0f;
    private AudioSource audioSource;
    private PlayerController playerController;
    private float defaultFov;

    private void Awake()
    {
        if (firePoint == null)
        {
            Transform foundFirePoint = transform.Find("PM_Firepoint");
            if (foundFirePoint != null)
                firePoint = foundFirePoint;
        }

        if (playerCamera == null)
            playerCamera = Camera.main;

        audioSource = GetComponent<AudioSource>();
        playerController = GetComponentInParent<PlayerController>();

        if (playerController == null && transform.root != null)
            playerController = transform.root.GetComponent<PlayerController>();

        if (playerCamera != null)
            defaultFov = playerCamera.fieldOfView;

        if (zoomUiImage != null)
            zoomUiImage.enabled = false;
    }

    private void Update()
    {
        // Gameplay-Input blockieren, wenn das Spiel pausiert ist.
        // UI/Maus für das Pausemenü bleiben dabei trotzdem aktiv.
        if (Time.timeScale == 0f)
            return;

        if (Mouse.current == null)
            return;

        HandleZoom();

        if (fullAuto)
        {
            HandleFullAutoFire();
        }
        else
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryShoot(shotCooldown);
            }
        }
    }

    private void HandleZoom()
    {
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

        bool isZooming = Mouse.current.rightButton.isPressed;
        float targetFov = isZooming ? zoomFov : defaultFov;

        playerCamera.fieldOfView = Mathf.Lerp(
            playerCamera.fieldOfView,
            targetFov,
            zoomSmoothSpeed * Time.deltaTime
        );

        if (zoomUiImage != null)
            zoomUiImage.enabled = isZooming;
    }

    private void HandleFullAutoFire()
    {
        if (!Mouse.current.leftButton.isPressed)
            return;

        float autoShotInterval = fireRate > 0f ? 1f / fireRate : 0.01f;
        TryShoot(autoShotInterval);
    }

    private void TryShoot(float cooldown)
    {
        // Zweite Sicherheit, falls TryShoot später mal von woanders aufgerufen wird.
        if (Time.timeScale == 0f)
            return;

        if (Time.time < nextShotTime)
            return;

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

        Vector3 targetPoint = GetAimTargetPoint();
        Vector3 shotDirection = (targetPoint - firePoint.position).normalized;

        if (shotDirection.sqrMagnitude < 0.0001f)
            shotDirection = firePoint.forward;

        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

        GameObject projectileInstance = Instantiate(
            projectilePrefab,
            firePoint.position,
            shotRotation
        );

        ProjectileController projectile = projectileInstance.GetComponent<ProjectileController>();
        if (projectile != null)
        {
            Vector3 inheritedVelocity = playerController != null ? playerController.WorldMoveVelocity : Vector3.zero;
            projectile.Initialize(transform.root.gameObject, inheritedVelocity);
        }

        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        EnemyBrain.NotifyGunshot(firePoint.position);

        nextShotTime = Time.time + cooldown;

        if (debugLogShoot)
        {
            Debug.Log($"{name}: Shot fired toward {targetPoint}");
        }

        if (debugDrawShotRay)
        {
            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
        }
    }

    private Vector3 GetAimTargetPoint()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, maxAimDistance, aimMask, QueryTriggerInteraction.Ignore))
        {
            return hit.point;
        }

        return ray.GetPoint(maxAimDistance);
    }
}