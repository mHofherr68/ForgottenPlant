/*using UnityEngine;

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
}*/
/*using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField, Range(0f, 180f)] private float viewAngle = 90f;

    [Header("Layers")]
    [SerializeField] private LayerMask detectionMask;

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

        Vector3 playerTarget = player.position + Vector3.up * 1.0f;
        Vector3 toPlayer = playerTarget - eyePoint.position;

        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer > viewDistance)
            return false;

        Vector3 directionToPlayer = toPlayer.normalized;

        float angle = Vector3.Angle(eyePoint.forward, directionToPlayer);
        if (angle > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(
                eyePoint.position,
                directionToPlayer,
                out RaycastHit hit,
                distanceToPlayer,
                detectionMask,
                QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * eyePoint.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePoint.position, left * viewDistance);
        Gizmos.DrawRay(eyePoint.position, right * viewDistance);

        if (CanSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, player.position + Vector3.up * 1.0f);
        }
    }
}*/
/*using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField, Range(0f, 180f)] private float viewAngle = 90f;

    [Header("Layers")]
    [SerializeField] private LayerMask detectionMask;

    [Header("Debug")]
    [SerializeField] private bool debugLogDetection = true;

    public bool CanSeePlayer { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; private set; }

    private bool wasSeeingPlayerLastFrame = false;

    private void Update()
    {
        CanSeePlayer = CheckPlayerVisibility();

        if (CanSeePlayer && player != null)
        {
            LastKnownPlayerPosition = player.position;
        }

        bool justDetectedPlayer = CanSeePlayer && !wasSeeingPlayerLastFrame;

        if (justDetectedPlayer)
        {
            if (debugLogDetection)
            {
                Debug.Log($"{name} sees the player!");
            }
        }

        wasSeeingPlayerLastFrame = CanSeePlayer;
    }

    private bool CheckPlayerVisibility()
    {
        if (player == null || eyePoint == null)
            return false;

        Vector3 playerTarget = player.position + Vector3.up * 1.0f;
        Vector3 toPlayer = playerTarget - eyePoint.position;

        float distanceToPlayer = toPlayer.magnitude;
        if (distanceToPlayer > viewDistance)
            return false;

        Vector3 directionToPlayer = toPlayer.normalized;

        float angle = Vector3.Angle(eyePoint.forward, directionToPlayer);
        if (angle > viewAngle * 0.5f)
            return false;

        if (Physics.Raycast(
                eyePoint.position,
                directionToPlayer,
                out RaycastHit hit,
                distanceToPlayer,
                detectionMask,
                QueryTriggerInteraction.Ignore))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                return true;
            }

            return false;
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (eyePoint == null)
            return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        Vector3 left = Quaternion.Euler(0, -viewAngle / 2f, 0) * eyePoint.forward;
        Vector3 right = Quaternion.Euler(0, viewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePoint.position, left * viewDistance);
        Gizmos.DrawRay(eyePoint.position, right * viewDistance);

        if (CanSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, player.position + Vector3.up * 1.0f);
        }
    }
}*/
/*using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField, Range(0f, 180f)] private float focusViewAngle = 90f;
    [SerializeField, Range(0f, 180f)] private float peripheralViewAngle = 140f;

    [Header("Layers")]
    [SerializeField] private LayerMask detectionMask;

    [Header("Debug")]
    [SerializeField] private bool debugLogDetection = true;

    public bool CanSeePlayer { get; private set; }
    public bool HasSuspicion { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; private set; }

    private bool wasSeeingPlayerLastFrame = false;
    private bool wasSuspiciousLastFrame = false;

    private void Update()
    {
        bool canSee = CheckVision(focusViewAngle);
        bool suspicious = CheckVision(peripheralViewAngle);

        CanSeePlayer = canSee;
        HasSuspicion = !canSee && suspicious;

        if ((CanSeePlayer || HasSuspicion) && player != null)
        {
            LastKnownPlayerPosition = player.position;
        }

        if (CanSeePlayer && !wasSeeingPlayerLastFrame)
        {
            if (debugLogDetection)
                Debug.Log($"{name}: sees the player!");
        }

        if (HasSuspicion && !wasSuspiciousLastFrame)
        {
            if (debugLogDetection)
                Debug.Log($"{name}: Hmm... something there?");
        }

        wasSeeingPlayerLastFrame = CanSeePlayer;
        wasSuspiciousLastFrame = HasSuspicion;
    }

    private bool CheckVision(float angleLimit)
    {
        if (player == null || eyePoint == null)
            return false;

        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 toPlayer = target - eyePoint.position;

        float distance = toPlayer.magnitude;
        if (distance > viewDistance)
            return false;

        Vector3 direction = toPlayer.normalized;

        float angle = Vector3.Angle(eyePoint.forward, direction);
        if (angle > angleLimit * 0.5f)
            return false;

        if (Physics.Raycast(eyePoint.position, direction, out RaycastHit hit, distance, detectionMask))
        {
            return hit.transform == player || hit.transform.IsChildOf(player);
        }

        return false;
    }
}*/
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform eyePoint;

    [Header("Vision Settings")]
    [SerializeField] private float viewDistance = 15f;
    [SerializeField, Range(0f, 180f)] private float focusViewAngle = 90f;
    [SerializeField, Range(0f, 180f)] private float peripheralViewAngle = 140f;

    [Header("Layers")]
    [SerializeField] private LayerMask detectionMask;

    [Header("Debug")]
    [SerializeField] private bool debugLogDetection = true;
    [SerializeField] private bool debugDrawGizmos = true;

    public bool CanSeePlayer { get; private set; }
    public bool HasSuspicion { get; private set; }
    public Vector3 LastKnownPlayerPosition { get; private set; }

    private bool wasSeeingPlayerLastFrame = false;
    private bool wasSuspiciousLastFrame = false;

    private void Update()
    {
        bool canSee = CheckVision(focusViewAngle);
        bool suspicious = CheckVision(peripheralViewAngle);

        CanSeePlayer = canSee;
        HasSuspicion = !canSee && suspicious;

        if ((CanSeePlayer || HasSuspicion) && player != null)
        {
            LastKnownPlayerPosition = player.position;
        }

        if (CanSeePlayer && !wasSeeingPlayerLastFrame)
        {
            if (debugLogDetection)
                Debug.Log($"{name}: sees the player!");
        }

        if (HasSuspicion && !wasSuspiciousLastFrame)
        {
            if (debugLogDetection)
                Debug.Log($"{name}: Hmm... something there?");
        }

        wasSeeingPlayerLastFrame = CanSeePlayer;
        wasSuspiciousLastFrame = HasSuspicion;
    }

    private bool CheckVision(float angleLimit)
    {
        if (player == null || eyePoint == null)
            return false;

        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 toPlayer = target - eyePoint.position;

        float distance = toPlayer.magnitude;
        if (distance > viewDistance)
            return false;

        Vector3 direction = toPlayer.normalized;

        float angle = Vector3.Angle(eyePoint.forward, direction);
        if (angle > angleLimit * 0.5f)
            return false;

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
        if (!debugDrawGizmos)
            return;

        if (eyePoint == null)
            return;

        // 🔵 Sichtweite
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        // 🔷 Fokusbereich (ALARM)
        Vector3 leftFocus = Quaternion.Euler(0, -focusViewAngle / 2f, 0) * eyePoint.forward;
        Vector3 rightFocus = Quaternion.Euler(0, focusViewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePoint.position, leftFocus * viewDistance);
        Gizmos.DrawRay(eyePoint.position, rightFocus * viewDistance);

        // 🟣 Peripherie (HMM)
        Vector3 leftPeripheral = Quaternion.Euler(0, -peripheralViewAngle / 2f, 0) * eyePoint.forward;
        Vector3 rightPeripheral = Quaternion.Euler(0, peripheralViewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(eyePoint.position, leftPeripheral * viewDistance);
        Gizmos.DrawRay(eyePoint.position, rightPeripheral * viewDistance);

        // 🔴 Linie bei direkter Sicht
        if (CanSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, player.position + Vector3.up * 1.0f);
        }
    }
}
