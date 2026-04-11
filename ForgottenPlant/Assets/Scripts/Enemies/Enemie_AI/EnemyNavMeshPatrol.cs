using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class EnemyNavMeshPatrol : MonoBehaviour
{
    [Header("Speed Settings")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float alarmRunSpeed = 6.0f;

    [Header("Alarm Brake")]
    [SerializeField] private float alarmApproachSpeed = 2.5f;
    [SerializeField] private float alarmSlowdownDistance = 2.0f;

    [Header("References")]
    [SerializeField] private PatrolRoute patrolRoute;
    [SerializeField] private EnemyDetection detection;
    [SerializeField] private EnemieBrain brain;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    [Header("Investigation")]
    [SerializeField] private float investigateWaitTime = 3f;

    [Header("Suspicion Delay")]
    [SerializeField] private float suspicionDelay = 4f;

    [Header("Alarm Point")]
    [SerializeField] private float alarmPointReachedDistance = 0.5f;
    [SerializeField] private bool debugLogAlarmPoint = true;

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private bool isRunningToAlarmPoint = false;
    private bool hasTriggeredMasterAlarm = false;

    // Investigation
    private bool isInvestigating = false;
    private float investigateTimer = 0f;

    // Suspicion Delay
    private bool suspicionActive = false;
    private float suspicionTimer = 0f;
    private bool wasSuspicionDetectedLastFrame = false;

    // OffMeshLink
    private bool isTraversingOffMeshLink = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;

        if (detection == null)
            detection = GetComponent<EnemyDetection>();

        if (brain == null)
            brain = GetComponent<EnemieBrain>();
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
        if (!isTraversingOffMeshLink && agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseOffMeshLinkRoutine());
            return;
        }

        if (isTraversingOffMeshLink)
            return;

        bool hasSuspicionNow = detection != null && detection.HasSuspicion;

        // =========================
        // Suspicion Trigger (mehrfach möglich)
        // =========================
        if (!isRunningToAlarmPoint && hasSuspicionNow && !wasSuspicionDetectedLastFrame)
        {
            if (brain != null)
                brain.OnSuspicionStarted();

            if (!suspicionActive && !isInvestigating)
            {
                suspicionActive = true;
                suspicionTimer = suspicionDelay;

                Debug.Log($"{name}: Hmm... waiting {suspicionDelay}s before investigating");
            }
        }

        wasSuspicionDetectedLastFrame = hasSuspicionNow;

        // =========================
        // Suspicion (Hmm + Delay)
        // =========================
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
        // Investigate
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
        // Alarm
        // =========================
        if (isRunningToAlarmPoint)
        {
            HandleAlarmApproachBrake();
            CheckAlarmPointReached();
            return;
        }

        // =========================
        // Patrol
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

    private IEnumerator TraverseOffMeshLinkRoutine()
    {
        isTraversingOffMeshLink = true;

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        Vector3 startPos = transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * agent.baseOffset;

        agent.updatePosition = false;

        float traverseSpeed = Mathf.Max(0.01f, agent.speed);
        float distance = Vector3.Distance(startPos, endPos);
        float duration = distance / traverseSpeed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            Vector3 nextPos = Vector3.Lerp(startPos, endPos, t);
            transform.position = nextPos;

            Vector3 flatDir = endPos - transform.position;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            yield return null;
        }

        transform.position = endPos;

        agent.CompleteOffMeshLink();
        agent.updatePosition = true;
        agent.Warp(transform.position);

        ResumeCurrentMovementState();

        isTraversingOffMeshLink = false;
    }

    private void ResumeCurrentMovementState()
    {
        if (isRunningToAlarmPoint)
        {
            if (AlarmSystem.Instance != null && AlarmSystem.Instance.AlarmPoint != null)
            {
                agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
            }

            return;
        }

        if (isInvestigating)
        {
            if (detection != null)
            {
                agent.SetDestination(detection.LastKnownPlayerPosition);
            }

            return;
        }

        if (patrolRoute != null && patrolRoute.Count > 0 && !isWaiting)
        {
            MoveToPoint(currentIndex);
        }
    }

    private void HandleAlarmApproachBrake()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= alarmSlowdownDistance)
            agent.speed = alarmApproachSpeed;
        else
            agent.speed = alarmRunSpeed;
    }

    private void CheckAlarmPointReached()
    {
        if (hasTriggeredMasterAlarm)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > alarmPointReachedDistance)
            return;

        hasTriggeredMasterAlarm = true;

        // bewusst TRUE lassen, damit er nicht zurück in Patrol fällt,
        // sondern erstmal am AlarmPoint stehen bleibt
        isRunningToAlarmPoint = true;

        agent.ResetPath();

        if (debugLogAlarmPoint)
            Debug.Log($"{name}: Reached AlarmPoint -> MASTERALARM");

        AlarmSystem.Instance?.TriggerAlarm();
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
        hasTriggeredMasterAlarm = false;
        isInvestigating = false;
        suspicionActive = false;
        isWaiting = false;

        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }
}