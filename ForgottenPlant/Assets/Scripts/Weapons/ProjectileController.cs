using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ProjectileController : MonoBehaviour
{
    [Header("Projectile Settings")]
    // Base forward speed of the projectile.
    [SerializeField] private float speed = 25f;

    // Maximum lifetime before the projectile is destroyed automatically.
    [SerializeField] private float lifeTime = 5f;

    // Multiplier used for inherited shooter movement velocity.
    [SerializeField] private float inheritedVelocityIntensity = 1f;

    [Header("Damage")]
    // Damage dealt when the projectile hits a valid target.
    [SerializeField] private int projectileDamage = 1;

    [Header("Hit Settings")]
    // Layer mask used to determine which objects the projectile can hit.
    [SerializeField] private LayerMask hitMask;

    [Header("Near Impact")]
    // Radius used to notify nearby enemies about an impact.
    [SerializeField] private float nearImpactRadius = 4f;

    [Header("Debug")]
    // Enables debug logs for direct hit events.
    [SerializeField] private bool debugLogHits = true;

    // Enables debug logs for near-impact notifications.
    [SerializeField] private bool debugLogNearImpact = false;

    [Header("Projectile VFX")]
    // Optional impact visual effect prefab.
    [SerializeField] private GameObject impactVfxPrefab;

    // Small offset used to place the impact VFX slightly above the hit surface.
    [SerializeField] private float impactOffset = 0.02f;

    [Header("Audio")]
    // Optional impact sound played when the projectile hits something.
    [SerializeField] private AudioClip impactClip;

    // Cached Rigidbody used for projectile movement.
    private Rigidbody rb;

    // Cached projectile collider.
    private Collider ownCollider;

    // Object that fired this projectile.
    private GameObject owner;

    // Prevents multiple hit handling.
    private bool hasHit = false;

    // Cached AudioSource used for impact playback.
    private AudioSource audioSource;

    // Cached renderers used to hide the projectile after impact.
    private Renderer[] renderers;

    // Velocity inherited from the shooter.
    private Vector3 inheritedVelocity = Vector3.zero;

    private void Awake()
    {
        // Cache core components.
        rb = GetComponent<Rigidbody>();
        ownCollider = GetComponent<Collider>();
        audioSource = GetComponent<AudioSource>();
        renderers = GetComponentsInChildren<Renderer>();

        // Configure the projectile rigidbody for fast trigger-based movement.
        rb.useGravity = false;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Warn if the collider is not configured as a trigger.
        if (!ownCollider.isTrigger)
        {
            Debug.LogWarning($"{name}: Collider should use 'Is Trigger' for this projectile setup.");
        }
    }

    private void Start()
    {
        // Split inherited velocity into forward and lateral parts.
        Vector3 forwardInheritedVelocity = Vector3.Project(inheritedVelocity, transform.forward);
        Vector3 lateralInheritedVelocity = inheritedVelocity - forwardInheritedVelocity;

        // Ignore backward inherited velocity so the projectile is not slowed down unnaturally.
        if (Vector3.Dot(forwardInheritedVelocity, transform.forward) < 0f)
            forwardInheritedVelocity = Vector3.zero;

        Vector3 finalInheritedVelocity = forwardInheritedVelocity + lateralInheritedVelocity;

        // Apply final launch velocity.
        rb.linearVelocity = transform.forward * speed + finalInheritedVelocity * inheritedVelocityIntensity;

        // Destroy the projectile automatically after its lifetime expires.
        Destroy(gameObject, lifeTime);
    }

    public void Initialize(GameObject projectileOwner, Vector3 ownerVelocity)
    {
        // Store the owner and the inherited movement velocity.
        owner = projectileOwner;
        inheritedVelocity = ownerVelocity;

        if (owner == null)
            return;

        // Ignore collisions with all colliders that belong to the owner.
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
        // Ignore repeated hit handling.
        if (hasHit)
            return;

        if (other == null)
            return;

        // Ignore collisions with the owner root object.
        if (owner != null && other.gameObject == owner)
            return;

        // Ignore layers that are not part of the configured hit mask.
        if (!IsLayerInMask(other.gameObject.layer, hitMask))
            return;

        hasHit = true;

        if (debugLogHits)
        {
            Debug.Log($"{name}: Hit {other.name} on layer '{LayerMask.LayerToName(other.gameObject.layer)}'");
        }

        // Apply damage to enemies if an enemy health controller is found.
        EnemyHealthController enemyHealth = other.GetComponentInParent<EnemyHealthController>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(projectileDamage);
        }

        // Apply damage to the player if a player health controller is found.
        PlayerHealthController playerHealth = other.GetComponentInParent<PlayerHealthController>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(projectileDamage);
        }

        // Spawn impact VFX and notify nearby enemies about the impact location.
        Vector3 impactPosition = SpawnImpactVfx(other);
        EnemyBrain.NotifyNearImpact(impactPosition, nearImpactRadius);

        if (debugLogNearImpact)
        {
            Debug.Log($"{name}: NearImpact sent at {impactPosition} with radius {nearImpactRadius}");
        }

        // Disable the projectile collider after impact.
        if (ownCollider != null)
            ownCollider.enabled = false;

        // Stop projectile physics motion completely.
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.isKinematic = true;
        }

        // Hide all projectile renderers after impact.
        if (renderers != null)
        {
            foreach (Renderer r in renderers)
            {
                if (r != null)
                    r.enabled = false;
            }
        }

        // Play impact sound if available, then destroy after the clip finishes.
        if (audioSource != null && impactClip != null)
        {
            audioSource.PlayOneShot(impactClip);
            Destroy(gameObject, impactClip.length);
        }
        else
        {
            // If no sound is available, destroy immediately.
            Destroy(gameObject);
        }
    }

    private Vector3 SpawnImpactVfx(Collider other)
    {
        // Use current projectile velocity direction if available, otherwise fallback to forward.
        Vector3 direction = rb.linearVelocity.sqrMagnitude > 0.001f
            ? rb.linearVelocity.normalized
            : transform.forward;

        Vector3 rayStart = transform.position - direction * 0.1f;
        Vector3 spawnPosition = transform.position;
        Quaternion spawnRotation = Quaternion.LookRotation(-direction);

        // Try to place the impact VFX directly on the hit surface using a short raycast.
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

        // Spawn the impact VFX if one is assigned.
        if (impactVfxPrefab != null)
        {
            Instantiate(impactVfxPrefab, spawnPosition, spawnRotation);
        }

        return spawnPosition;
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        // Returns true if the given layer is included in the layer mask.
        return (mask.value & (1 << layer)) != 0;
    }
}