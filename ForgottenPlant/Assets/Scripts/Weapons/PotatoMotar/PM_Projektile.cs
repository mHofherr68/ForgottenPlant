/*using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PM_Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Hit Settings")]
    [SerializeField] private LayerMask hitMask;

    [Header("Debug")]
    [SerializeField] private bool debugLogHits = true;

    private Rigidbody rb;
    private Collider ownCollider;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (!ownCollider.isTrigger)
        {
            Debug.LogWarning($"{name}: Collider should use 'Is Trigger' for this projectile setup.");
        }
    }

    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(GameObject projectileOwner)
    {
        owner = projectileOwner;

        if (owner == null)
            return;

        Collider ownerCollider = owner.GetComponent<Collider>();
        if (ownerCollider == null)
            ownerCollider = owner.GetComponentInChildren<Collider>();

        if (ownerCollider != null)
        {
            Physics.IgnoreCollision(ownCollider, ownerCollider);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (owner != null)
        {
            if (other.gameObject == owner || other.transform.IsChildOf(owner.transform))
                return;
        }

        if (!IsLayerInMask(other.gameObject.layer, hitMask))
            return;

        if (debugLogHits)
        {
            Debug.Log($"{name}: Hit {other.name} on layer '{LayerMask.LayerToName(other.gameObject.layer)}'");
        }

        // Später hier:
        // - VFX / Verpuffung
        // - Damage
        // - Sound

        Destroy(gameObject);
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}*/
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PM_Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Hit Settings")]
    [SerializeField] private LayerMask hitMask;

    [Header("Debug")]
    [SerializeField] private bool debugLogHits = true;

    [Header("Projectile VFX")]
    [SerializeField] private GameObject impactVfxPrefab;
    [SerializeField] private float impactOffset = 0.02f;

    private Rigidbody rb;
    private Collider ownCollider;
    private GameObject owner;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();

        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        if (!ownCollider.isTrigger)
        {
            Debug.LogWarning($"{name}: Collider should use 'Is Trigger' for this projectile setup.");
        }
    }

    private void Start()
    {
        rb.linearVelocity = transform.forward * speed;
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(GameObject projectileOwner)
    {
        owner = projectileOwner;

        if (owner == null)
            return;

        Collider ownerCollider = owner.GetComponent<Collider>();
        if (ownerCollider == null)
            ownerCollider = owner.GetComponentInChildren<Collider>();

        if (ownerCollider != null)
        {
            Physics.IgnoreCollision(ownCollider, ownerCollider);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other == null)
            return;

        if (owner != null)
        {
            if (other.gameObject == owner || other.transform.IsChildOf(owner.transform))
                return;
        }

        if (!IsLayerInMask(other.gameObject.layer, hitMask))
            return;

        if (debugLogHits)
        {
            Debug.Log($"{name}: Hit {other.name} on layer '{LayerMask.LayerToName(other.gameObject.layer)}'");
        }

        SpawnImpactVfx(other);

        // Später hier:
        // - Damage
        // - Sound

        Destroy(gameObject);
    }

    private void SpawnImpactVfx(Collider other)
    {
        if (impactVfxPrefab == null)
            return;

        Vector3 direction = rb.linearVelocity.sqrMagnitude > 0.001f
            ? rb.linearVelocity.normalized
            : transform.forward;

        Vector3 rayStart = transform.position - direction * 0.1f;
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = Quaternion.LookRotation(-direction);

        if (Physics.Raycast(
            rayStart,
            direction,
            out RaycastHit hit,
            1.0f,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            if (hit.collider == other || hit.transform.IsChildOf(other.transform))
            {
                spawnPosition = hit.point + hit.normal * impactOffset;
                spawnRotation = Quaternion.LookRotation(hit.normal);
            }
        }

        Instantiate(impactVfxPrefab, spawnPosition, spawnRotation);
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }
}