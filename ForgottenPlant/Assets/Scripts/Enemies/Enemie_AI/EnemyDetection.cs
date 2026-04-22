using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("References")]
    // Reference to the player transform that should be detected.
    [SerializeField] private Transform player;

    // Reference point from which the enemy performs vision checks.
    [SerializeField] private Transform eyePoint;

    [Header("Vision Settings")]
    // Maximum distance at which the player can be detected.
    [SerializeField] private float viewDistance = 15f;

    // Narrower field of view used for direct visibility.
    [SerializeField, Range(0f, 180f)] private float focusViewAngle = 90f;

    // Wider field of view used for suspicious peripheral detection.
    [SerializeField, Range(0f, 180f)] private float peripheralViewAngle = 140f;

    [Header("Layers")]
    // Layer mask used for line-of-sight raycasts.
    [SerializeField] private LayerMask detectionMask;

    [Header("Debug")]
    // Enables debug logs when the player is seen or suspected.
    [SerializeField] private bool debugLogDetection = true;

    // Enables gizmo visualization for the enemy vision cones.
    [SerializeField] private bool debugDrawGizmos = true;

    // True if the player is currently inside the focused direct vision cone.
    public bool CanSeePlayer { get; private set; }

    // True if the player is not directly seen, but is detected in peripheral vision.
    public bool HasSuspicion { get; private set; }

    // Stores the last known player position whenever the player is seen or suspected.
    public Vector3 LastKnownPlayerPosition { get; private set; }

    // Used to detect state changes for debug logging.
    private bool wasSeeingPlayerLastFrame = false;
    private bool wasSuspiciousLastFrame = false;

    public void SetPlayer(Transform playerTransform)
    {
        // Allows assigning the player transform externally.
        player = playerTransform;
    }

    private void Update()
    {
        // Run direct and peripheral vision checks every frame.
        bool canSee = CheckVision(focusViewAngle);
        bool suspicious = CheckVision(peripheralViewAngle);

        // Direct sight has priority over peripheral suspicion.
        CanSeePlayer = canSee;
        HasSuspicion = !canSee && suspicious;

        // Update the last known player position whenever the player is detected in any way.
        if ((CanSeePlayer || HasSuspicion) && player != null)
        {
            LastKnownPlayerPosition = player.position;
        }

        // Log when the player becomes directly visible.
        if (CanSeePlayer && !wasSeeingPlayerLastFrame)
        {
            if (debugLogDetection)
                Debug.Log($"{name}: sees the player!");
        }

        // Log when the player is only detected suspiciously.
        if (HasSuspicion && !wasSuspiciousLastFrame)
        {
            if (debugLogDetection)
                Debug.Log($"{name}: Hmm... something there?");
        }

        // Store current state for next frame comparison.
        wasSeeingPlayerLastFrame = CanSeePlayer;
        wasSuspiciousLastFrame = HasSuspicion;
    }

    private bool CheckVision(float angleLimit)
    {
        // Detection cannot work without a player or eye reference.
        if (player == null || eyePoint == null)
            return false;

        // Aim the check slightly above the player's origin to better match body height.
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 toPlayer = target - eyePoint.position;

        float distance = toPlayer.magnitude;
        if (distance > viewDistance)
            return false;

        Vector3 direction = toPlayer.normalized;

        // Reject the target if it is outside the current field of view.
        float angle = Vector3.Angle(eyePoint.forward, direction);
        if (angle > angleLimit * 0.5f)
            return false;

        // Perform a line-of-sight raycast to ensure the player is not blocked by geometry.
        if (Physics.Raycast(
            eyePoint.position,
            direction,
            out RaycastHit hit,
            distance,
            detectionMask,
            QueryTriggerInteraction.Ignore))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        // Skip all debug drawing if disabled.
        if (!debugDrawGizmos)
            return;

        if (eyePoint == null)
            return;

        // Draw the general view distance radius.
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        // Draw the focused vision cone edges.
        Vector3 leftFocus = Quaternion.Euler(0, -focusViewAngle / 2f, 0) * eyePoint.forward;
        Vector3 rightFocus = Quaternion.Euler(0, focusViewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePoint.position, leftFocus * viewDistance);
        Gizmos.DrawRay(eyePoint.position, rightFocus * viewDistance);

        // Draw the peripheral vision cone edges.
        Vector3 leftPeripheral = Quaternion.Euler(0, -peripheralViewAngle / 2f, 0) * eyePoint.forward;
        Vector3 rightPeripheral = Quaternion.Euler(0, peripheralViewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(eyePoint.position, leftPeripheral * viewDistance);
        Gizmos.DrawRay(eyePoint.position, rightPeripheral * viewDistance);

        // Draw the current line to the player if the player is directly visible.
        if (CanSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, player.position + Vector3.up * 1.0f);
        }
    }
}