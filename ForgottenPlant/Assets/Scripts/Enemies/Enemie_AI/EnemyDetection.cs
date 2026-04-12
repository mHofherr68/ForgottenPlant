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

    // ✅ NEU
    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
    }

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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(eyePoint.position, viewDistance);

        Vector3 leftFocus = Quaternion.Euler(0, -focusViewAngle / 2f, 0) * eyePoint.forward;
        Vector3 rightFocus = Quaternion.Euler(0, focusViewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(eyePoint.position, leftFocus * viewDistance);
        Gizmos.DrawRay(eyePoint.position, rightFocus * viewDistance);

        Vector3 leftPeripheral = Quaternion.Euler(0, -peripheralViewAngle / 2f, 0) * eyePoint.forward;
        Vector3 rightPeripheral = Quaternion.Euler(0, peripheralViewAngle / 2f, 0) * eyePoint.forward;

        Gizmos.color = Color.magenta;
        Gizmos.DrawRay(eyePoint.position, leftPeripheral * viewDistance);
        Gizmos.DrawRay(eyePoint.position, rightPeripheral * viewDistance);

        if (CanSeePlayer && player != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(eyePoint.position, player.position + Vector3.up * 1.0f);
        }
    }
}