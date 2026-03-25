using UnityEngine;

public class PatrolRoute : MonoBehaviour
{
    public enum RouteMode
    {
        Loop,
        PingPong
    }

    [System.Serializable]
    public class PatrolPoint
    {
        public Transform point;
        public float waitTime = 2f;
    }

    [Header("Route Settings")]
    [SerializeField] private RouteMode routeMode = RouteMode.Loop;
    [SerializeField] private PatrolPoint[] patrolPoints;

    public RouteMode Mode => routeMode;
    public PatrolPoint[] Points => patrolPoints;

    public int Count => patrolPoints != null ? patrolPoints.Length : 0;

    public Transform GetPointTransform(int index)
    {
        if (!IsValidIndex(index))
            return null;

        return patrolPoints[index].point;
    }

    public float GetPointWaitTime(int index)
    {
        if (!IsValidIndex(index))
            return 0f;

        return patrolPoints[index].waitTime;
    }

    private bool IsValidIndex(int index)
    {
        return patrolPoints != null &&
               index >= 0 &&
               index < patrolPoints.Length;
    }
}
