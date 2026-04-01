using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshPatrol : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (patrolRoute == null)
        {
            Debug.LogWarning($"{name}: No PatrolRoute assigned.");
            enabled = false;
            return;
        }

        if (patrolRoute.Count == 0)
        {
            Debug.LogWarning($"{name}: PatrolRoute has no patrol points.");
            enabled = false;
            return;
        }

        MoveToPoint(currentIndex);
    }

    private void Update()
    {
        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                AdvanceToNextPoint();
            }

            return;
        }

        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= pointReachedDistance)
        {
            StartWaiting();
        }
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = patrolRoute.GetPointWaitTime(currentIndex);
        agent.ResetPath();
    }

    private void AdvanceToNextPoint()
    {
        if (patrolRoute.Mode == PatrolRoute.RouteMode.Loop)
        {
            currentIndex = (currentIndex + 1) % patrolRoute.Count;
        }
        else if (patrolRoute.Mode == PatrolRoute.RouteMode.PingPong)
        {
            currentIndex += direction;

            if (currentIndex >= patrolRoute.Count)
            {
                direction = -1;
                currentIndex = patrolRoute.Count - 2;
            }
            else if (currentIndex < 0)
            {
                direction = 1;
                currentIndex = 1;
            }
        }

        MoveToPoint(currentIndex);
    }

    private void MoveToPoint(int index)
    {
        Transform targetPoint = patrolRoute.GetPointTransform(index);

        if (targetPoint == null)
        {
            Debug.LogWarning($"{name}: Patrol point at index {index} is missing.");
            return;
        }

        agent.SetDestination(targetPoint.position);
    }
}
