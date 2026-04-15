//using UnityEngine;

//public class WeaponObstacleCheck : MonoBehaviour
//{
//    [Header("Detection")]
//    [SerializeField] private LayerMask obstacleMask;
//    [SerializeField] private float sphereRadius = 0.2f;

//    [Header("Movement")]
//    [SerializeField] private float pullToCenterX = 0.12f;
//    [SerializeField] private float pullBackZ = 0.08f;
//    [SerializeField] private float smoothSpeed = 8f;

//    [Header("Debug")]
//    [SerializeField] private bool debugDrawGizmos = true;

//    public float CurrentOffsetX { get; private set; }
//    public float CurrentOffsetZ { get; private set; }

//    private void LateUpdate()
//    {
//        bool blocked = Physics.CheckSphere(
//            transform.position,
//            sphereRadius,
//            obstacleMask,
//            QueryTriggerInteraction.Ignore
//        );

//        float targetOffsetX = blocked ? -pullToCenterX : 0f;
//        float targetOffsetZ = blocked ? -pullBackZ : 0f;

//        CurrentOffsetX = Mathf.Lerp(
//            CurrentOffsetX,
//            targetOffsetX,
//            smoothSpeed * Time.deltaTime
//        );

//        CurrentOffsetZ = Mathf.Lerp(
//            CurrentOffsetZ,
//            targetOffsetZ,
//            smoothSpeed * Time.deltaTime
//        );
//    }

//    private void OnDrawGizmosSelected()
//    {
//        if (!debugDrawGizmos)
//            return;

//        Gizmos.color = Color.yellow;
//        Gizmos.DrawWireSphere(transform.position, sphereRadius);
//    }
//}
using UnityEngine;

public class WeaponObstacleCheck : MonoBehaviour
{
    [Header("Detection")]
    [SerializeField] private LayerMask obstacleMask;
    [SerializeField] private float sphereRadius = 0.2f;
    [SerializeField] private float releaseSphereRadius = 0.24f;

    [Header("Movement")]
    [SerializeField] private float pullToCenterX = 0.12f;
    [SerializeField] private float pullBackZ = 0.08f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Debug")]
    [SerializeField] private bool debugDrawGizmos = true;

    public float CurrentOffsetX { get; private set; }
    public float CurrentOffsetZ { get; private set; }

    private bool isBlocked = false;

    private void LateUpdate()
    {
        // ===== HYSTERESE-CHECK =====
        float activeRadius = isBlocked ? releaseSphereRadius : sphereRadius;

        isBlocked = Physics.CheckSphere(
            transform.position,
            activeRadius,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );
        // ===== HYSTERESE-CHECK ENDE =====

        float targetOffsetX = isBlocked ? -pullToCenterX : 0f;
        float targetOffsetZ = isBlocked ? -pullBackZ : 0f;

        CurrentOffsetX = Mathf.Lerp(
            CurrentOffsetX,
            targetOffsetX,
            smoothSpeed * Time.deltaTime
        );

        CurrentOffsetZ = Mathf.Lerp(
            CurrentOffsetZ,
            targetOffsetZ,
            smoothSpeed * Time.deltaTime
        );
    }

    private void OnDrawGizmosSelected()
    {
        if (!debugDrawGizmos)
            return;

        Gizmos.color = isBlocked ? Color.red : Color.yellow;

        float gizmoRadius = isBlocked ? releaseSphereRadius : sphereRadius;
        Gizmos.DrawWireSphere(transform.position, gizmoRadius);
    }
}