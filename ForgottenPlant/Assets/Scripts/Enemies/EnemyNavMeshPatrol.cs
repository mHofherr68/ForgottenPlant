/*using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshPatrol : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float alarmRunSpeed = 6.0f;

    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool isRunningToAlarmPoint = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        agent.speed = patrolSpeed;

        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        MoveToPoint(currentIndex);
    }

    private void Update()
    {
        if (isRunningToAlarmPoint)
            return;

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

    public void RunToAlarmPoint()
    {
        if (AlarmSystem.Instance == null || AlarmSystem.Instance.AlarmPoint == null)
        {
            Debug.LogWarning($"{name}: No AlarmSystem or AlarmPoint assigned.");
            return;
        }

        isRunningToAlarmPoint = true;
        isWaiting = false;
        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }
}*/
/*using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshPatrol : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float alarmRunSpeed = 6.0f;

    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;
    [SerializeField] private EnemyDetection detection;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    [Header("Suspicion")]
    [SerializeField] private float investigateWaitTime = 3f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private bool isRunningToAlarmPoint = false;
    private bool isInvestigating = false;
    private float investigateTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<EnemyDetection>();
    }

    private void Start()
    {
        agent.speed = patrolSpeed;

        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        MoveToPoint(currentIndex);
    }

    private void Update()
    {
        // 🔍 Suspicion → Investigate
        if (!isRunningToAlarmPoint && !isInvestigating && detection != null && detection.HasSuspicion)
        {
            isInvestigating = true;
            isWaiting = false;

            agent.ResetPath();
            agent.speed = patrolSpeed;
            agent.SetDestination(detection.LastKnownPlayerPosition);

            investigateTimer = investigateWaitTime;

            Debug.Log($"{name}: Investigating...");
        }

        // 🔍 Investigate Verhalten
        if (isInvestigating)
        {
            if (!agent.pathPending && agent.remainingDistance <= pointReachedDistance)
            {
                investigateTimer -= Time.deltaTime;

                if (investigateTimer <= 0f)
                {
                    isInvestigating = false;

                    Debug.Log($"{name}: Nothing found, back to patrol.");

                    MoveToPoint(currentIndex);
                }
            }
            return;
        }

        // 🚨 Alarm
        if (isRunningToAlarmPoint)
            return;

        // 🚶 Patrol
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
        else
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
        Transform target = patrolRoute.GetPointTransform(index);
        if (target != null)
        {
            agent.SetDestination(target.position);
        }
    }

    public void RunToAlarmPoint()
    {
        if (AlarmSystem.Instance == null || AlarmSystem.Instance.AlarmPoint == null)
            return;

        isRunningToAlarmPoint = true;
        isInvestigating = false;

        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }
}*/
using UnityEngine;
using UnityEngine.AI;

public class EnemyNavMeshPatrol : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float alarmRunSpeed = 6.0f;

    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;
    [SerializeField] private EnemyDetection detection;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    [Header("Investigation")]
    [SerializeField] private float investigateWaitTime = 3f;

    [Header("Suspicion Delay")]
    [SerializeField] private float suspicionDelay = 4f;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private bool isRunningToAlarmPoint = false;

    // 🧠 Investigation
    private bool isInvestigating = false;
    private float investigateTimer = 0f;

    // 🧠 Suspicion Delay
    private bool suspicionActive = false;
    private float suspicionTimer = 0f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<EnemyDetection>();
    }

    private void Start()
    {
        agent.speed = patrolSpeed;

        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        MoveToPoint(currentIndex);
    }

    private void Update()
    {
        // =========================
        // 🟡 Suspicion (Hmm + Delay)
        // =========================
        if (!isRunningToAlarmPoint && !isInvestigating && detection != null && detection.HasSuspicion)
        {
            if (!suspicionActive)
            {
                suspicionActive = true;
                suspicionTimer = suspicionDelay;

                Debug.Log($"{name}: Hmm... waiting {suspicionDelay}s before investigating");
            }
        }

        if (suspicionActive)
        {
            suspicionTimer -= Time.deltaTime;

            if (suspicionTimer <= 0f)
            {
                suspicionActive = false;
                StartInvestigation(detection.LastKnownPlayerPosition);
            }

            return;
        }

        // =========================
        // 🔍 Investigate
        // =========================
        if (isInvestigating)
        {
            if (!agent.pathPending && agent.remainingDistance <= pointReachedDistance)
            {
                investigateTimer -= Time.deltaTime;

                if (investigateTimer <= 0f)
                {
                    isInvestigating = false;

                    Debug.Log($"{name}: Nothing found, back to patrol.");

                    agent.speed = patrolSpeed;
                    MoveToPoint(currentIndex);
                }
            }

            return;
        }

        // =========================
        // 🚨 Alarm
        // =========================
        if (isRunningToAlarmPoint)
            return;

        // =========================
        // 🚶 Patrol
        // =========================
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

    private void StartInvestigation(Vector3 position)
    {
        isInvestigating = true;
        isWaiting = false;

        agent.ResetPath();
        agent.speed = patrolSpeed;
        agent.SetDestination(position);

        investigateTimer = investigateWaitTime;

        Debug.Log($"{name}: Investigating...");
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
        else
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
        Transform target = patrolRoute.GetPointTransform(index);

        if (target == null)
        {
            Debug.LogWarning($"{name}: Patrol point at index {index} is missing.");
            return;
        }

        agent.SetDestination(target.position);
    }

    public void RunToAlarmPoint()
    {
        if (AlarmSystem.Instance == null || AlarmSystem.Instance.AlarmPoint == null)
        {
            Debug.LogWarning($"{name}: No AlarmSystem or AlarmPoint assigned.");
            return;
        }

        isRunningToAlarmPoint = true;
        isInvestigating = false;
        suspicionActive = false;

        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }
}
