using UnityEngine;

public class WeaponObstacleCheck : MonoBehaviour
{
    [Header("Detection")]
    // Layer mask used to detect nearby obstacles in front of the weapon.
    [SerializeField] private LayerMask obstacleMask;

    // Sphere radius used for the initial obstacle detection.
    [SerializeField] private float sphereRadius = 0.2f;

    // Slightly larger radius used while already blocked
    // to create hysteresis and avoid rapid flickering.
    [SerializeField] private float releaseSphereRadius = 0.24f;

    [Header("Movement")]
    // Horizontal offset used to pull the weapon toward the center when blocked.
    [SerializeField] private float pullToCenterX = 0.12f;

    // Backward offset used to pull the weapon away from nearby obstacles.
    [SerializeField] private float pullBackZ = 0.08f;

    // Smoothing speed for obstacle offset transitions.
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Debug")]
    // Enables gizmo visualization for obstacle detection.
    [SerializeField] private bool debugDrawGizmos = true;

    // Current horizontal obstacle offset used by the weapon sway system.
    public float CurrentOffsetX { get; private set; }

    // Current backward obstacle offset used by the weapon sway system.
    public float CurrentOffsetZ { get; private set; }

    // True while the weapon is currently considered blocked by an obstacle.
    private bool isBlocked = false;

    private void LateUpdate()
    {
        // ===== HYSTERESIS CHECK =====
        // Use a larger radius while already blocked
        // so the weapon does not constantly switch between blocked and unblocked states.
        float activeRadius = isBlocked ? releaseSphereRadius : sphereRadius;

        isBlocked = Physics.CheckSphere(
            transform.position,
            activeRadius,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );
        // ===== END HYSTERESIS CHECK =====

        // Build target offsets depending on whether an obstacle is detected.
        float targetOffsetX = isBlocked ? -pullToCenterX : 0f;
        float targetOffsetZ = isBlocked ? -pullBackZ : 0f;

        // Smoothly interpolate the current X offset.
        CurrentOffsetX = Mathf.Lerp(
            CurrentOffsetX,
            targetOffsetX,
            smoothSpeed * Time.deltaTime
        );

        // Smoothly interpolate the current Z offset.
        CurrentOffsetZ = Mathf.Lerp(
            CurrentOffsetZ,
            targetOffsetZ,
            smoothSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        // Stop if debug drawing is disabled.
        if (!debugDrawGizmos)
            return;

        // Draw the sphere in red while blocked, otherwise yellow.
        Gizmos.color = isBlocked ? Color.red : Color.yellow;

        // Visualize the currently active detection radius.
        float gizmoRadius = isBlocked ? releaseSphereRadius : sphereRadius;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
}