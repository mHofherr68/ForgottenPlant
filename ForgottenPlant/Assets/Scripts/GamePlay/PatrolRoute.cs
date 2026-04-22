using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    // Defines how the patrol route should be traversed.
    public enum RouteMode
    {
        // After the last point, continue again from the first point.
        Loop,

        // Move back and forth between the first and last point.
        PingPong
    }

    [System.Serializable]
    public class PatrolPoint
    {
        // World transform of this patrol point.
        public Transform point;

        // Time the enemy should wait after reaching this point.
        public float waitTime = 2f;
    }

    [Header("Route Settings")]
    // Determines whether the route loops or uses ping-pong movement.
    [SerializeField] private RouteMode routeMode = RouteMode.Loop;

    // Ordered list of patrol points used by the patrol system.
    [SerializeField] private PatrolPoint[] patrolPoints;

    // Public read-only access to the selected route mode.
    public RouteMode Mode => routeMode;

    // Public read-only access to the patrol point array.
    public PatrolPoint[] Points => patrolPoints;

    // Returns the number of configured patrol points.
    public int Count => patrolPoints != null ? patrolPoints.Length : 0;

    public Transform GetPointTransform(int index)
    {
        // Return null if the requested index is invalid.
        if (!IsValidIndex(index))
            return null;

        // Return the transform stored at the given patrol point index.
        return patrolPoints[index].point;
    }

    public float GetPointWaitTime(int index)
    {
        // Return zero if the requested index is invalid.
        if (!IsValidIndex(index))
            return 0f;

        // Return the configured wait time for the given patrol point.
        return patrolPoints[index].waitTime;
    }

    private bool IsValidIndex(int index)
    {
        // Check whether the patrol point array exists
        // and whether the given index is inside the valid range.
        return patrolPoints != null &&
               index >= 0 &&
               index < patrolPoints.Length;
    }
}