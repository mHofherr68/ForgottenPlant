using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PM_Projectile : MonoBehaviour
{
    [Header("Projectile Settings")]
    [SerializeField] private float speed = 25f;
    [SerializeField] private float lifeTime = 5f;

    [Header("Damage")]
    [SerializeField] private int projectileDamage = 1;

    [Header("Hit Settings")]
    [SerializeField] private LayerMask hitMask;

    [Header("Debug")]
    [SerializeField] private bool debugLogHits = true;

    [Header("Projectile VFX")]
    [SerializeField] private GameObject impactVfxPrefab;
    [SerializeField] private float impactOffset = 0.02f;

    [Header("Audio")]
    [SerializeField] private AudioClip impactClip;

    private Rigidbody rb;
    private Collider ownCollider;
    private GameObject owner;
    private bool hasHit = false;
    private AudioSource audioSource;
    private Renderer[] renderers;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        renderers = GetComponentsInChildren<Renderer>();

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

        Collider[] ownerColliders = owner.GetComponentsInChildren<Collider>();

        foreach (Collider col in ownerColliders)
        {
            if (col != null)
            {
                Physics.IgnoreCollision(ownCollider, col);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit)
            return;

        if (other == null)
            return;

        if (owner != null && other.gameObject == owner)
            return;

        if (!IsLayerInMask(other.gameObject.layer, hitMask))
            return;

        hasHit = true;

        if (debugLogHits)
        {
            Debug.Log($"{name}: Hit {other.name} on layer '{LayerMask.LayerToName(other.gameObject.layer)}'");
        }

        EnemyHealthController enemyHealth = other.GetComponentInParent<EnemyHealthController>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(projectileDamage);
        }

        SpawnImpactVfx(other);

        if (ownCollider != null)
            ownCollider.enabled = false;

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = false;
            }
        }

        if (audioSource != null && impactClip != null)
        {
            audioSource.PlayOneShot(impactClip);
            Destroy(gameObject, impactClip.length);
        }
        else
        {
            Destroy(gameObject);
        }
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