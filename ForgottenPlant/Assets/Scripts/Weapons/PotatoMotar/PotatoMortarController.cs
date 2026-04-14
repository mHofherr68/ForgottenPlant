using UnityEngine;
using UnityEngine.InputSystem;

public class PotatoMortarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Camera playerCamera;

    [Header("Shoot Settings")]
    [SerializeField] private float shotCooldown = 0.6f;
    [SerializeField] private float maxAimDistance = 100f;
    [SerializeField] private LayerMask aimMask = ~0;

    [Header("Debug")]
    [SerializeField] private bool debugLogShoot = false;
    [SerializeField] private bool debugDrawShotRay = false;

    private float nextShotTime = 0f;

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
    }

    private void Update()
    {
        // Gameplay-Input blockieren, wenn das Spiel pausiert ist.
        // UI/Maus für das Pausemenü bleiben dabei trotzdem aktiv.
        if (Time.timeScale == 0f)
            return;

        if (Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryShoot();
        }
    }

    private void TryShoot()
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

        PM_Projectile projectile = projectileInstance.GetComponent<PM_Projectile>();
        if (projectile != null)
        {
            projectile.Initialize(transform.root.gameObject);
        }

        nextShotTime = Time.time + shotCooldown;

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
