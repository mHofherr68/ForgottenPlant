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

    private bool isInvestigating = false;
    private float investigateTimer = 0f;

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
        // 🚨 Running to Alarm Point
        // =========================
        if (isRunningToAlarmPoint)
        {
            CheckAlarmPointReached();
            return;
        }

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

    private void CheckAlarmPointReached()
    {
        if (hasTriggeredMasterAlarm)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > alarmPointReachedDistance)
            return;

        hasTriggeredMasterAlarm = true;
        isRunningToAlarmPoint = true; // -- false

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
    private bool isHoldingAtAlarmPoint = false;
    private bool hasTriggeredMasterAlarm = false;

    private bool isInvestigating = false;
    private float investigateTimer = 0f;

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
        // Enemy ist am AlarmPoint angekommen und soll erstmal dort stehen bleiben
        if (isHoldingAtAlarmPoint)
            return;

        // =========================
        // Suspicion (Hmm + Delay)
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
        // Running to Alarm Point
        // =========================
        if (isRunningToAlarmPoint)
        {
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

    private void CheckAlarmPointReached()
    {
        if (hasTriggeredMasterAlarm)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > alarmPointReachedDistance)
            return;

        hasTriggeredMasterAlarm = true;
        isRunningToAlarmPoint = false;
        isHoldingAtAlarmPoint = true;

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
        isHoldingAtAlarmPoint = false;
        hasTriggeredMasterAlarm = false;

        isInvestigating = false;
        suspicionActive = false;
        isWaiting = false;

        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }

    // Für später:
    // Wenn Attack eingebaut wird, kann der Enemy den AlarmPoint verlassen.
    public void StartAttackMode()
    {
        isHoldingAtAlarmPoint = false;
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
    [SerializeField] private EnemyVoice voice;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    [Header("Investigation")]
    [SerializeField] private float investigateWaitTime = 3f;

    [Header("Suspicion Delay")]
    [SerializeField] private float suspicionDelay = 4f;

    [Header("Alarm Point")]
    [SerializeField] private float alarmPointReachedDistance = 0.5f;
    [SerializeField] private bool debugLogAlarmPoint = true;

    [Header("Voice Index")]
    [SerializeField] private int suspicionVoiceIndex = 0; // 0 = Hmm

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private bool isRunningToAlarmPoint = false;
    private bool isHoldingAtAlarmPoint = false;
    private bool hasTriggeredMasterAlarm = false;

    private bool isInvestigating = false;
    private float investigateTimer = 0f;

    private bool suspicionActive = false;
    private float suspicionTimer = 0f;
    private bool hasPlayedSuspicionVoice = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<EnemyDetection>();

        if (voice == null)
            voice = GetComponent<EnemyVoice>();
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
        // Enemy ist am AlarmPoint angekommen und soll erstmal dort stehen bleiben
        if (isHoldingAtAlarmPoint)
            return;

        // =========================
        // Suspicion (Hmm + Delay)
        // =========================
        if (!isRunningToAlarmPoint && !isInvestigating && detection != null && detection.HasSuspicion)
        {
            if (!suspicionActive)
            {
                suspicionActive = true;
                suspicionTimer = suspicionDelay;
                hasPlayedSuspicionVoice = false;

                if (!hasPlayedSuspicionVoice && voice != null)
                {
                    voice.PlayVoice(suspicionVoiceIndex);
                    hasPlayedSuspicionVoice = true;
                }

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
        // Running to Alarm Point
        // =========================
        if (isRunningToAlarmPoint)
        {
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

    private void CheckAlarmPointReached()
    {
        if (hasTriggeredMasterAlarm)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > alarmPointReachedDistance)
            return;

        hasTriggeredMasterAlarm = true;
        isRunningToAlarmPoint = false;
        isHoldingAtAlarmPoint = true;

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
        isHoldingAtAlarmPoint = false;
        hasTriggeredMasterAlarm = false;

        isInvestigating = false;
        suspicionActive = false;
        isWaiting = false;

        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }

    // Für später:
    // Wenn Attack eingebaut wird, kann der Enemy den AlarmPoint verlassen.
    public void StartAttackMode()
    {
        isHoldingAtAlarmPoint = false;
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
    [SerializeField] private EnemyVoice voice;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    [Header("Investigation")]
    [SerializeField] private float investigateWaitTime = 3f;

    [Header("Suspicion Delay")]
    [SerializeField] private float suspicionDelay = 4f;

    [Header("Alarm Point")]
    [SerializeField] private float alarmPointReachedDistance = 0.5f;
    [SerializeField] private bool debugLogAlarmPoint = true;

    [Header("Voice Index")]
    [SerializeField] private int suspicionVoiceIndex = 0; // 0 = Hmm

    private NavMeshAgent agent;
    private int currentIndex = 0;
    private int direction = 1;

    private float waitTimer = 0f;
    private bool isWaiting = false;

    private bool isRunningToAlarmPoint = false;
    private bool isHoldingAtAlarmPoint = false;
    private bool hasTriggeredMasterAlarm = false;

    private bool isInvestigating = false;
    private float investigateTimer = 0f;

    private bool suspicionActive = false;
    private float suspicionTimer = 0f;
    private bool hasPlayedSuspicionVoice = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<EnemyDetection>();

        if (voice == null)
            voice = GetComponent<EnemyVoice>();
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
        // Enemy ist am AlarmPoint angekommen und soll erstmal dort stehen bleiben
        if (isHoldingAtAlarmPoint)
            return;

        // =========================
        // Suspicion (Hmm + Delay)
        // =========================
        if (!isRunningToAlarmPoint && !isInvestigating && detection != null && detection.HasSuspicion)
        {
            if (!suspicionActive)
            {
                suspicionActive = true;
                suspicionTimer = suspicionDelay;
                hasPlayedSuspicionVoice = false;

                if (!hasPlayedSuspicionVoice && voice != null)
                {
                    voice.PlayVoice(suspicionVoiceIndex);
                    hasPlayedSuspicionVoice = true;
                }

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
        // Running to Alarm Point
        // =========================
        if (isRunningToAlarmPoint)
        {
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

    private void CheckAlarmPointReached()
    {
        if (hasTriggeredMasterAlarm)
            return;

        if (AlarmSystem.Instance == null || AlarmSystem.Instance.AlarmPoint == null)
            return;

        Vector3 enemyPosition = transform.position;
        Vector3 alarmPointPosition = AlarmSystem.Instance.AlarmPoint.position;

        float distanceToAlarmPoint = Vector3.Distance(enemyPosition, alarmPointPosition);

        if (distanceToAlarmPoint > alarmPointReachedDistance)
            return;

        hasTriggeredMasterAlarm = true;
        isRunningToAlarmPoint = false;
        isHoldingAtAlarmPoint = true;

        agent.ResetPath();

        if (debugLogAlarmPoint)
            Debug.Log($"{name}: Reached AlarmPoint -> MASTERALARM (World Distance = {distanceToAlarmPoint:F2})");

        AlarmSystem.Instance.TriggerAlarm();
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
        isHoldingAtAlarmPoint = false;
        hasTriggeredMasterAlarm = false;

        isInvestigating = false;
        suspicionActive = false;
        isWaiting = false;

        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }

    // Für später:
    // Wenn Attack eingebaut wird, kann der Enemy den AlarmPoint verlassen.
    public void StartAttackMode()
    {
        isHoldingAtAlarmPoint = false;
    }
}*/
using UnityEngine;
using UnityEngine.AI;

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
    [SerializeField] private EnemyVoice voice;

    [Header("Movement Settings")]
    [SerializeField] private float pointReachedDistance = 0.25f;

    [Header("Investigation")]
    [SerializeField] private float investigateWaitTime = 3f;

    [Header("Suspicion Delay")]
    [SerializeField] private float suspicionDelay = 4f;

    [Header("Alarm Point")]
    [SerializeField] private float alarmPointReachedDistance = 0.5f;
    [SerializeField] private bool debugLogAlarmPoint = true;

    [Header("Voice Index")]
    [SerializeField] private int suspicionVoiceIndex = 0;

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
    private bool hasPlayedSuspicionVoice = false;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        detection = GetComponent<EnemyDetection>();

        if (voice == null)
            voice = GetComponent<EnemyVoice>();
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
        // Suspicion (Hmm + Delay)
        // =========================
        if (!isRunningToAlarmPoint && !isInvestigating && detection != null && detection.HasSuspicion)
        {
            if (!suspicionActive)
            {
                suspicionActive = true;
                suspicionTimer = suspicionDelay;
                hasPlayedSuspicionVoice = false;

                if (!hasPlayedSuspicionVoice && voice != null)
                {
                    voice.PlayVoice(suspicionVoiceIndex);
                    hasPlayedSuspicionVoice = true;
                }

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

        // WICHTIG:
        // true lassen, damit er nicht zurück in Patrol fällt,
        // sondern erstmal am AlarmPoint stehen bleibt.
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