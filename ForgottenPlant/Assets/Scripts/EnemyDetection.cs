using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField, Range(0f, 180f)] private float viewAngle = 90f;

    [Header("Layers")]
    [SerializeField] private LayerMask detectionMask; // Player + Ground + Obstacle (NICHT Enemy!)

    [Header("Debug")]
    [SerializeField] private bool debugLogDetection = true;

    public bool CanSeePlayer { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; private set; }

    private void Update()
    {
        CanSeePlayer = CheckPlayerVisibility();

        if (CanSeePlayer && player != null)
        {
            LastKnownPlayerPosition = player.position;

            if (debugLogDetection)
            {
                Debug.Log($"{name} sees the player!");
            }

            AlarmSystem.Instance?.TriggerAlarm();
        }
    }

    private bool CheckPlayerVisibility()
    {
        if (player == null || eyePoint == null)
            return false;

        Vector3 toPlayer = player.position - eyePoint.position;
        float distanceToPlayer = toPlayer.magnitude;

        // 1. Distanz check
        if (distanceToPlayer > viewDistance)
            return false;

        Vector3 directionToPlayer = toPlayer.normalized;

        // 2. Winkel check
        float angle = Vector3.Angle(eyePoint.forward, directionToPlayer);
        if (angle > viewAngle * 0.5f)
            return false;

        // 3. Raycast (sichtprüfung)
        if (Physics.Raycast(
                eyePoint.position,
                directionToPlayer,
                out RaycastHit hit,
                distanceToPlayer,
                detectionMask,
                QueryTriggerInteraction.Ignore))
        {
            // trifft Player?
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                return true;
            }

            // trifft etwas anderes → Sicht blockiert
            return false;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePoint == null)
            return;

        // Sichtweite
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        // Sichtwinkel
        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * eyePoint.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePoint.position, left * viewDistance);
        Gizmos.DrawRay(eyePoint.position, right * viewDistance);

        // Debug: Player gesehen
        if (CanSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, player.position);
        }
    }
}