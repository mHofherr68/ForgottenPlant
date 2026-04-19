//using UnityEngine;

//public class EnemyWeaponController : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private Transform firePoint;
//    [SerializeField] private GameObject projectilePrefab;
//    [SerializeField] private Transform target;

//    [Header("Audio")]
//    [SerializeField] private AudioClip shootClip;

//    [Header("Shoot Settings")]
//    [SerializeField] private float shotCooldown = 0.8f;

//    [Header("Debug")]
//    [SerializeField] private bool debugLogShoot = false;
//    [SerializeField] private bool debugDrawShotRay = false;

//    private float nextShotTime = 0f;
//    private AudioSource audioSource;

//    private void Awake()
//    {
//        if (firePoint == null)
//        {
//            Transform foundFirePoint = transform.Find("FirePoint");
//            if (foundFirePoint != null)
//                firePoint = foundFirePoint;
//        }

//        audioSource = GetComponent<AudioSource>();
//    }

//    public void SetTarget(Transform newTarget)
//    {
//        target = newTarget;
//    }

//    public bool CanFire()
//    {
//        return Time.time >= nextShotTime;
//    }

//    public bool TryFire()
//    {
//        if (Time.timeScale == 0f)
//            return false;

//        if (Time.time < nextShotTime)
//            return false;

//        if (projectilePrefab == null)
//        {
//            Debug.LogWarning($"{name}: No projectile prefab assigned.");
//            return false;
//        }

//        if (firePoint == null)
//        {
//            Debug.LogWarning($"{name}: No firePoint assigned.");
//            return false;
//        }

//        if (target == null)
//        {
//            Debug.LogWarning($"{name}: No target assigned.");
//            return false;
//        }

//        Vector3 shotDirection = (target.position - firePoint.position).normalized;

//        if (shotDirection.sqrMagnitude < 0.0001f)
//            shotDirection = firePoint.forward;

//        Quaternion shotRotation = Quaternion.LookRotation(shotDirection);

//        GameObject projectileInstance = Instantiate(
//            projectilePrefab,
//            firePoint.position,
//            shotRotation
//        );

//        PM_Projectile projectile = projectileInstance.GetComponent<PM_Projectile>();
//        if (projectile != null)
//        {
//            projectile.Initialize(transform.root.gameObject, Vector3.zero);
//        }

//        if (audioSource != null && shootClip != null)
//        {
//            audioSource.PlayOneShot(shootClip);
//        }

//        nextShotTime = Time.time + shotCooldown;

//        if (debugLogShoot)
//        {
//            Debug.Log($"{name}: Enemy shot fired toward {target.name}");
//        }

//        if (debugDrawShotRay)
//        {
//            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
//        }

//        return true;
//    }
//}
using UnityEngine;

public class EnemyWeaponController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform target;

    [Header("Aim Offset")]
    [SerializeField] private Vector3 targetOffset = new Vector3(0f, 1.2f, 0f);

    [Header("Audio")]
    [SerializeField] private AudioClip shootClip;

    [Header("Shoot Settings")]
    [SerializeField] private float shotCooldown = 0.8f;

    [Header("Debug")]
    [SerializeField] private bool debugLogShoot = false;
    [SerializeField] private bool debugDrawShotRay = false;

    private float nextShotTime = 0f;
    private AudioSource audioSource;

    private void Awake()
    {
        if (firePoint == null)
        {
            Transform foundFirePoint = transform.Find("FirePoint");
            if (foundFirePoint != null)
                firePoint = foundFirePoint;
        }

        audioSource = GetComponent<AudioSource>();
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public bool CanFire()
    {
        return Time.time >= nextShotTime;
    }

    public bool TryFire()
    {
        if (Time.timeScale == 0f)
            return false;

        if (Time.time < nextShotTime)
            return false;

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

        Vector3 aimPoint = target.position + targetOffset;
        Vector3 shotDirection = (aimPoint - firePoint.position).normalized;

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
            projectile.Initialize(transform.root.gameObject, Vector3.zero);
        }

        if (audioSource != null && shootClip != null)
        {
            audioSource.PlayOneShot(shootClip);
        }

        nextShotTime = Time.time + shotCooldown;

        if (debugLogShoot)
        {
            Debug.Log($"{name}: Enemy shot fired toward {target.name}");
        }

        if (debugDrawShotRay)
        {
            Debug.DrawRay(firePoint.position, shotDirection * 10f, Color.red, 1f);
        }

        return true;
    }
}