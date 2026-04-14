//using UnityEngine;

//public class WeaponObstacleCheck : MonoBehaviour
//{
//    [Header("Detection")]
//    [SerializeField] private LayerMask obstacleMask;
//    [SerializeField] private float sphereRadius = 0.2f;

//    [Header("Movement")]
//    [SerializeField] private float pullToCenterX = 0.12f;
//    [SerializeField] private float smoothSpeed = 8f;

//    [Header("Debug")]
//    [SerializeField] private bool debugDrawGizmos = true;

//    public float CurrentOffsetX { get; private set; }

//    private void LateUpdate()
//    {
//        bool blocked = Physics.CheckSphere(
//            transform.position,
//            sphereRadius,
//            obstacleMask,
//            QueryTriggerInteraction.Ignore
//        );

//        float targetOffsetX = blocked ? -pullToCenterX : 0f;

//        CurrentOffsetX = Mathf.Lerp(
//            CurrentOffsetX,
//            targetOffsetX,
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

    [Header("Movement")]
    [SerializeField] private float pullToCenterX = 0.12f;
    [SerializeField] private float pullBackZ = 0.08f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Debug")]
    [SerializeField] private bool debugDrawGizmos = true;

    public float CurrentOffsetX { get; private set; }
    public float CurrentOffsetZ { get; private set; }

    private void LateUpdate()
    {
        bool blocked = Physics.CheckSphere(
            transform.position,
            sphereRadius,
            obstacleMask,
            QueryTriggerInteraction.Ignore
        );

        float targetOffsetX = blocked ? -pullToCenterX : 0f;
        float targetOffsetZ = blocked ? -pullBackZ : 0f;

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

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
    }
}