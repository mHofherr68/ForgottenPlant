/*using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private PatrolRoute patrolRoute;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float reachDistance = 0.2f;
    [SerializeField] private float rotationSpeed = 5f;

    private int currentIndex = 0;
    private int direction = 1; // für PingPong
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Start()
    {
        if (patrolRoute == null || patrolRoute.Count == 0)
        {
            Debug.LogWarning("No PatrolRoute assigned or empty!");
            return;
        }

        waitTimer = patrolRoute.GetPointWaitTime(currentIndex);
        isWaiting = true;
    }

    private void Update()
    {
        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        if (isWaiting)
        {
            HandleWaiting();
            return;
        }

        Patrol();
    }

    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            isWaiting = false;
        }
    }

    private void Patrol()
    {
        Transform target = patrolRoute.GetPointTransform(currentIndex);

        if (target == null)
            return;

        Vector3 targetPos = new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z
        );

        Vector3 directionToTarget = (targetPos - transform.position).normalized;

        transform.position += directionToTarget * speed * Time.deltaTime;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= reachDistance)
        {
            SetNextPoint();
            waitTimer = patrolRoute.GetPointWaitTime(currentIndex);
            isWaiting = true;
        }
    }

    private void SetNextPoint()
    {
        if (patrolRoute.Mode == PatrolRoute.RouteMode.Loop)
        {
            currentIndex++;

            if (currentIndex >= patrolRoute.Count)
            {
                currentIndex = 0;
            }
        }
        else // PingPong
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
    }
}*/
using UnityEngine;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Route")]
    [SerializeField] private PatrolRoute patrolRoute;

    [Header("Movement")]
    [SerializeField] private float speed = 2f;
    [SerializeField] private float reachDistance = 0.2f;
    [SerializeField] private float rotationSpeed = 5f;

    private int currentIndex = 0;
    private int direction = 1;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Start()
    {
        if (patrolRoute == null || patrolRoute.Count == 0)
        {
            Debug.LogWarning("No PatrolRoute assigned or empty!");
            return;
        }

        waitTimer = patrolRoute.GetPointWaitTime(currentIndex);
        isWaiting = true;
    }

    private void Update()
    {
        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        if (isWaiting)
        {
            HandleWaiting();
            return;
        }

        Patrol();
    }

    private void HandleWaiting()
    {
        waitTimer -= Time.deltaTime;

        if (waitTimer <= 0f)
        {
            isWaiting = false;
        }
    }

    private void Patrol()
    {
        Transform target = patrolRoute.GetPointTransform(currentIndex);

        if (target == null)
            return;

        Vector3 targetPos = new Vector3(
            target.position.x,
            transform.position.y,
            target.position.z
        );

        Vector3 directionToTarget = (targetPos - transform.position).normalized;

        transform.position += directionToTarget * speed * Time.deltaTime;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Lerp(
                transform.rotation,
                lookRotation,
                rotationSpeed * Time.deltaTime
            );
        }

        float distance = Vector3.Distance(transform.position, targetPos);

        if (distance <= reachDistance)
        {
            waitTimer = patrolRoute.GetPointWaitTime(currentIndex);
            isWaiting = true;
            SetNextPoint();
        }
    }

    private void SetNextPoint()
    {
        if (patrolRoute.Mode == PatrolRoute.RouteMode.Loop)
        {
            currentIndex++;

            if (currentIndex >= patrolRoute.Count)
            {
                currentIndex = 0;
            }
        }
        else // PingPong
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
    }
}