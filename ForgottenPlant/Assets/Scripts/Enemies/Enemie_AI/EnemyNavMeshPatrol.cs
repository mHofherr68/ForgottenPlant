using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class EnemyNavMeshPatrol : MonoBehaviour
{
    [Header("Speed Settings")]
    // Default movement speed used during normal patrol.
    [SerializeField] private float patrolSpeed = 3.5f;

    // Movement speed used when running to the alarm point.
    [SerializeField] private float alarmRunSpeed = 6.0f;

    [Header("Attack Run Speed By Difficulty")]
    // Search / combat movement speed for easy difficulty.
    [SerializeField] private float attackRunSpeedEasy = 5.5f;

    // Search / combat movement speed for medium difficulty.
    [SerializeField] private float attackRunSpeedMedium = 6.0f;

    // Search / combat movement speed for hard difficulty.
    [SerializeField] private float attackRunSpeedHard = 6.5f;

    [Header("Alarm Brake")]
    // Reduced speed used when approaching the alarm point.
    [SerializeField] private float alarmApproachSpeed = 2.5f;

    // Distance at which the enemy starts slowing down near the alarm point.
    [SerializeField] private float alarmSlowdownDistance = 2.0f;

    [Header("Alarm Spacing")]
    // Minimum spacing to other enemies near the alarm point.
    [SerializeField] private float alarmEnemyMinDistance = 1.5f;

    [Header("References")]
    // Patrol route used for default movement.
    [SerializeField] private PatrolRoute patrolRoute;

    // Detection component used for suspicion and last known player position.
    [SerializeField] private EnemyDetection detection;

    // Brain component used for higher-level enemy logic.
    [SerializeField] private EnemyBrain brain;

    // Optional home position used when the enemy returns home.
    [SerializeField] private Transform homePos;

    [Header("Movement Settings")]
    // Distance threshold at which a patrol point is considered reached.
    [SerializeField] private float pointReachedDistance = 0.25f;

    [Header("Investigation")]
    // Time spent investigating once the target position is reached.
    [SerializeField] private float investigateWaitTime = 3f;

    // Rotation angle used for left/right look during investigation.
    [SerializeField] private float investigateSearchLookAngle = 45f;

    // Turn speed used while looking around during investigation.
    [SerializeField] private float investigateTurnSpeed = 180f;

    [Header("Suspicion Delay")]
    // Delay between first suspicion and actual investigation movement.
    [SerializeField] private float suspicionDelay = 4f;

    [Header("Home Return")]
    // Distance threshold for reaching the home position.
    [SerializeField] private float homeReachedDistance = 0.35f;

    // Rotation speed used while aligning to the home forward direction.
    [SerializeField] private float homeTurnSpeed = 8f;

    // Enables debug logs for returning home.
    [SerializeField] private bool debugLogHomeReturn = true;

    [Header("Alarm Point")]
    // Distance threshold for considering the alarm point reached.
    [SerializeField] private float alarmPointReachedDistance = 0.5f;

    // Enables debug logs for alarm point behavior.
    [SerializeField] private bool debugLogAlarmPoint = true;

    [Header("Search")]
    // Distance threshold for reaching the current search target.
    [SerializeField] private float searchReachedDistance = 0.5f;

    // Minimum distance needed before a new search target is considered different.
    [SerializeField] private float searchUpdateThreshold = 0.25f;

    // Radius around the search center used for random follow-up search points.
    [SerializeField] private float searchRadius = 4f;

    // Number of random NavMesh point attempts when expanding the search.
    [SerializeField] private int searchPointAttempts = 8;

    // Enables debug logs for search behavior.
    [SerializeField] private bool debugLogSearch = true;

    // Global list of all active patrol components, mainly used for alarm spacing checks.
    private static readonly List<EnemyNavMeshPatrol> allPatrols = new List<EnemyNavMeshPatrol>();

    // Main NavMeshAgent used for all movement.
    private NavMeshAgent agent;

    // Current patrol point index.
    private int currentIndex = 0;

    // Current patrol direction for ping-pong routes.
    private int direction = 1;

    // Wait timer used at patrol points.
    private float waitTimer = 0f;

    // True while standing still at a patrol point.
    private bool isWaiting = false;

    // True while moving to the alarm point.
    private bool isRunningToAlarmPoint = false;

    // Prevents triggering the master alarm multiple times.
    private bool hasTriggeredMasterAlarm = false;

    // True while moving to and processing an investigation.
    private bool isInvestigating = false;

    // Investigation timer after reaching the suspicious point.
    private float investigateTimer = 0f;

    // True once the enemy has arrived at the investigation point.
    private bool hasReachedInvestigationPoint = false;

    // Base rotation stored when investigation look-around starts.
    private Quaternion investigationBaseRotation;

    // Left investigation look rotation.
    private Quaternion investigationLeftRotation;

    // Right investigation look rotation.
    private Quaternion investigationRightRotation;

    // Current investigation phase: left, right, then back to base.
    private int investigationLookPhase = 0;

    // True while the suspicion delay countdown is active.
    private bool suspicionActive = false;

    // Countdown timer for suspicion delay.
    private float suspicionTimer = 0f;

    // Tracks whether suspicion was already active in the previous frame.
    private bool wasSuspicionDetectedLastFrame = false;

    // Stores the target position for delayed investigation.
    private Vector3 pendingInvestigationPosition;

    // True while manually traversing an off-mesh link.
    private bool isTraversingOffMeshLink = false;

    // True while returning to the home position.
    private bool isReturningHome = false;

    // True while searching the player's last known area.
    private bool isSearching = false;

    // Current active search destination.
    private Vector3 currentSearchPosition;

    // Center point around which random search points are generated.
    private Vector3 currentSearchCenter;

    // Fallback home values used if no home transform is assigned.
    private Vector3 fallbackHomePosition;
    private Quaternion fallbackHomeRotation;

    private void Awake()
    {
        // Register this patrol instance globally.
        if (!allPatrols.Contains(this))
            allPatrols.Add(this);

        // Cache and configure the NavMeshAgent.
        agent = GetComponent<NavMeshAgent>();
        agent.autoTraverseOffMeshLink = false;

        // Auto-assign missing references.
        if (detection == null)
            detection = GetComponent<EnemyDetection>();

        if (brain == null)
            brain = GetComponent<EnemyBrain>();

        // Store current transform as fallback home position/rotation.
        fallbackHomePosition = transform.position;
        fallbackHomeRotation = transform.rotation;
    }

    private void OnDestroy()
    {
        // Remove this patrol instance from the global list.
        allPatrols.Remove(this);
    }

    private void Start()
    {
        // Start with normal patrol speed.
        agent.speed = patrolSpeed;

        // Begin patrol immediately if a valid route exists.
        if (patrolRoute == null || patrolRoute.Count == 0)
            return;

        MoveToPoint(currentIndex);
    }

    private void Update()
    {
        // Start custom off-mesh link traversal when needed.
        if (!isTraversingOffMeshLink && agent.isOnOffMeshLink)
        {
            StartCoroutine(TraverseOffMeshLinkRoutine());
            return;
        }

        if (isTraversingOffMeshLink)
            return;

        // Detect the first frame of a new suspicion event.
        bool hasSuspicionNow = detection != null && detection.HasSuspicion;

        if (!isRunningToAlarmPoint && !isSearching && hasSuspicionNow && !wasSuspicionDetectedLastFrame)
        {
            // Inform the brain that suspicion has started.
            if (brain != null)
                brain.OnSuspicionStarted();

            // Cache the suspicious position for delayed investigation.
            pendingInvestigationPosition = detection.LastKnownPlayerPosition;

            suspicionActive = true;
            suspicionTimer = suspicionDelay;

            // Cancel ongoing investigation and restart from the new suspicious position.
            if (isInvestigating)
            {
                isInvestigating = false;
                hasReachedInvestigationPoint = false;
                investigationLookPhase = 0;
                agent.ResetPath();
                agent.speed = patrolSpeed;
            }

            Debug.Log($"{name}: Hmm... waiting {suspicionDelay}s before investigating");
        }

        // Store suspicion state for edge detection next frame.
        wasSuspicionDetectedLastFrame = hasSuspicionNow;

        // Process suspicion countdown.
        if (suspicionActive)
        {
            suspicionTimer -= Time.deltaTime;

            if (suspicionTimer <= 0f)
            {
                suspicionActive = false;
                StartInvestigation(pendingInvestigationPosition);
            }

            return;
        }

        // Process investigation state.
        if (isInvestigating)
        {
            HandleInvestigation();
            return;
        }

        // Process return-home state.
        if (isReturningHome)
        {
            HandleReturnHome();
            return;
        }

        // Process alarm point running state.
        if (isRunningToAlarmPoint)
        {
            HandleAlarmApproachBrake();
            CheckAlarmPointReached();
            return;
        }

        // Process combat/search movement state.
        if (isSearching)
        {
            HandleSearch();
            return;
        }

        // No valid patrol route means nothing else to do.
        if (!HasValidPatrolRoute())
            return;

        // Process waiting at patrol points.
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

        // Start point wait logic once the patrol point is reached.
        if (agent.remainingDistance <= pointReachedDistance)
        {
            StartWaiting();
        }
    }

    public void SetPatrolRoute(PatrolRoute route)
    {
        // Assign a new patrol route at runtime.
        patrolRoute = route;

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (!HasValidPatrolRoute())
            return;

        // Reset patrol-related state.
        currentIndex = 0;
        direction = 1;
        isWaiting = false;
        isInvestigating = false;
        hasReachedInvestigationPoint = false;
        investigationLookPhase = 0;
        isReturningHome = false;
        suspicionActive = false;
        isSearching = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.speed = patrolSpeed;
        }

        MoveToPoint(currentIndex);
    }

    public void SetHomePosition(Transform homeTransform)
    {
        // Assign a new home transform at runtime.
        homePos = homeTransform;
    }

    private IEnumerator TraverseOffMeshLinkRoutine()
    {
        // Begin manual off-mesh link traversal.
        isTraversingOffMeshLink = true;

        OffMeshLinkData linkData = agent.currentOffMeshLinkData;

        Vector3 startPos = transform.position;
        Vector3 endPos = linkData.endPos + Vector3.up * agent.baseOffset;

        // Disable agent-driven position updates during custom traversal.
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

            // Rotate smoothly toward the link end position while moving.
            Vector3 flatDir = endPos - transform.position;
            flatDir.y = 0f;
            if (flatDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(flatDir.normalized);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);
            }

            yield return null;
        }

        // Finish the off-mesh link and resync agent position.
        transform.position = endPos;

        agent.CompleteOffMeshLink();
        agent.updatePosition = true;
        agent.Warp(transform.position);

        // Resume whichever movement state was active before traversal.
        ResumeCurrentMovementState();

        isTraversingOffMeshLink = false;
    }

    private void ResumeCurrentMovementState()
    {
        // Restore movement based on the currently active state.
        if (isRunningToAlarmPoint)
        {
            if (AlarmSystem.Instance != null && AlarmSystem.Instance.AlarmPoint != null)
            {
                agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
            }

            return;
        }

        if (isSearching)
        {
            agent.SetDestination(currentSearchPosition);
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

        if (isReturningHome)
        {
            agent.SetDestination(GetHomePosition());
            return;
        }

        if (HasValidPatrolRoute() && !isWaiting)
        {
            MoveToPoint(currentIndex);
        }
    }

    private void HandleAlarmApproachBrake()
    {
        if (agent.pathPending)
            return;

        // Slow down near the alarm point or if another enemy is already ahead.
        bool shouldBrakeForAlarmPoint = agent.remainingDistance <= alarmSlowdownDistance;
        bool shouldBrakeForEnemy = HasEnemyAheadNearAlarmPoint();

        if (shouldBrakeForAlarmPoint || shouldBrakeForEnemy)
            agent.speed = alarmApproachSpeed;
        else
            agent.speed = alarmRunSpeed;
    }

    private bool HasEnemyAheadNearAlarmPoint()
    {
        // Check if another enemy is already closer to the alarm point.
        if (AlarmSystem.Instance == null || AlarmSystem.Instance.AlarmPoint == null)
            return false;

        Vector3 alarmPoint = AlarmSystem.Instance.AlarmPoint.position;
        float myDistanceToAlarm = Vector3.Distance(transform.position, alarmPoint);

        if (myDistanceToAlarm > alarmSlowdownDistance)
            return false;

        for (int i = 0; i < allPatrols.Count; i++)
        {
            EnemyNavMeshPatrol other = allPatrols[i];

            if (other == null)
                continue;

            if (other == this)
                continue;

            if (!other.isRunningToAlarmPoint)
                continue;

            float otherDistanceToAlarm = Vector3.Distance(other.transform.position, alarmPoint);

            if (otherDistanceToAlarm >= myDistanceToAlarm)
                continue;

            float distanceToOther = Vector3.Distance(transform.position, other.transform.position);
            if (distanceToOther <= alarmEnemyMinDistance)
                return true;
        }

        return false;
    }

    private void CheckAlarmPointReached()
    {
        // Trigger the master alarm once the local alarm point has been reached.
        if (hasTriggeredMasterAlarm)
            return;

        if (agent.pathPending)
            return;

        if (agent.remainingDistance > alarmPointReachedDistance)
            return;

        hasTriggeredMasterAlarm = true;
        isRunningToAlarmPoint = true;

        agent.ResetPath();

        if (debugLogAlarmPoint)
            Debug.Log($"{name}: Reached AlarmPoint -> MASTERALARM");

        AlarmSystem.Instance?.TriggerAlarm();
    }

    private void StartInvestigation(Vector3 position)
    {
        // Start moving to the suspicious position.
        isInvestigating = true;
        isWaiting = false;
        isReturningHome = false;
        hasReachedInvestigationPoint = false;
        investigationLookPhase = 0;

        agent.ResetPath();
        agent.speed = patrolSpeed;
        agent.SetDestination(position);

        investigateTimer = investigateWaitTime;

        Debug.Log($"{name}: Investigating...");
    }

    private void HandleInvestigation()
    {
        if (!hasReachedInvestigationPoint)
        {
            if (agent.pathPending)
                return;

            if (agent.remainingDistance > pointReachedDistance)
                return;

            // Once the point is reached, stop and prepare look-around rotations.
            hasReachedInvestigationPoint = true;
            agent.ResetPath();

            investigationBaseRotation = transform.rotation;
            investigationLeftRotation = investigationBaseRotation * Quaternion.Euler(0f, -investigateSearchLookAngle, 0f);
            investigationRightRotation = investigationBaseRotation * Quaternion.Euler(0f, investigateSearchLookAngle, 0f);
            investigationLookPhase = 0;
            return;
        }

        Quaternion targetRotation = investigationBaseRotation;

        // Cycle through left look, right look, then back to base forward.
        switch (investigationLookPhase)
        {
            case 0:
                targetRotation = investigationLeftRotation;
                break;
            case 1:
                targetRotation = investigationRightRotation;
                break;
            default:
                targetRotation = investigationBaseRotation;
                break;
        }

        transform.rotation = Quaternion.RotateTowards(
            transform.rotation,
            targetRotation,
            investigateTurnSpeed * Time.deltaTime
        );

        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        if (angle < 1f)
        {
            investigationLookPhase++;
        }

        investigateTimer -= Time.deltaTime;

        if (investigateTimer <= 0f)
        {
            // Finish investigation and either resume patrol or return home.
            isInvestigating = false;
            hasReachedInvestigationPoint = false;
            investigationLookPhase = 0;

            if (HasValidPatrolRoute())
            {
                Debug.Log($"{name}: Nothing found, back to patrol.");

                agent.speed = patrolSpeed;
                MoveToPoint(currentIndex);
            }
            else
            {
                Debug.Log($"{name}: Nothing found, returning home.");
                ReturnToHome();
            }
        }
    }

    private void HandleSearch()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance > searchReachedDistance)
            return;

        // Once the current search point is reached, try to expand the search with a random nearby point.
        if (TrySetRandomSearchPoint())
            return;

        agent.ResetPath();

        if (debugLogSearch)
            Debug.Log($"{name}: Reached search position and no new random point found.");
    }

    private bool TrySetRandomSearchPoint()
    {
        // Generate random NavMesh points around the search center.
        if (searchRadius <= 0.01f)
            return false;

        int attempts = Mathf.Max(1, searchPointAttempts);

        for (int i = 0; i < attempts; i++)
        {
            Vector2 randomCircle = Random.insideUnitCircle * searchRadius;
            Vector3 rawPoint = new Vector3(
                currentSearchCenter.x + randomCircle.x,
                currentSearchCenter.y,
                currentSearchCenter.z + randomCircle.y
            );

            if (NavMesh.SamplePosition(rawPoint, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                // Ignore nearly identical search points.
                if (Vector3.Distance(currentSearchPosition, hit.position) <= searchUpdateThreshold)
                    continue;

                currentSearchPosition = hit.position;

                agent.ResetPath();
                agent.isStopped = false;
                agent.speed = GetAttackRunSpeedByDifficulty();
                agent.SetDestination(currentSearchPosition);

                if (debugLogSearch)
                    Debug.Log($"{name}: New random search point -> {currentSearchPosition}");

                return true;
            }
        }

        return false;
    }

    private void ReturnToHome()
    {
        // Begin return-home movement.
        isReturningHome = true;
        isWaiting = false;

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.speed = patrolSpeed;
            agent.SetDestination(GetHomePosition());
        }

        if (debugLogHomeReturn)
            Debug.Log($"{name}: Returning to home position.");
    }

    private void HandleReturnHome()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance > homeReachedDistance)
            return;

        // Stop movement and rotate to the home forward direction.
        agent.ResetPath();

        Quaternion targetRotation = GetHomeRotation();
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * homeTurnSpeed);

        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        if (angle < 2f)
        {
            transform.rotation = targetRotation;
            isReturningHome = false;

            if (debugLogHomeReturn)
                Debug.Log($"{name}: Reached home position and aligned to home forward.");
        }
    }

    private void StartWaiting()
    {
        // Start waiting at the current patrol point.
        if (!HasValidPatrolRoute())
            return;

        isWaiting = true;
        waitTimer = patrolRoute.GetPointWaitTime(currentIndex);
        agent.ResetPath();
    }

    private void AdvanceToNextPoint()
    {
        // Advance to the next patrol point depending on route mode.
        if (!HasValidPatrolRoute())
            return;

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
        // Move to the patrol point at the given index.
        if (!HasValidPatrolRoute())
            return;

        Transform target = patrolRoute.GetPointTransform(index);

        if (target == null)
        {
            Debug.LogWarning($"{name}: Patrol point at index {index} is missing.");
            return;
        }

        agent.SetDestination(target.position);
    }

    private bool HasValidPatrolRoute()
    {
        // A route is valid if it exists and contains at least one point.
        return patrolRoute != null && patrolRoute.Count > 0;
    }

    private Vector3 GetHomePosition()
    {
        // Use assigned home position if available, otherwise fallback to spawn position.
        return homePos != null ? homePos.position : fallbackHomePosition;
    }

    private Quaternion GetHomeRotation()
    {
        // Use assigned home rotation if available, otherwise fallback to spawn rotation.
        return homePos != null ? homePos.rotation : fallbackHomeRotation;
    }

    private float GetAttackRunSpeedByDifficulty()
    {
        // Read the current difficulty from the persistent settings system.
        int difficultyIndex = 0;

        if (GameSettingsManager.Instance != null && GameSettingsManager.Instance.CurrentSettings != null)
        {
            difficultyIndex = GameSettingsManager.Instance.CurrentSettings.difficultyIndex;
        }

        // Return the configured attack/search run speed for the current difficulty.
        switch (difficultyIndex)
        {
            case 0:
                return attackRunSpeedEasy;

            case 1:
                return attackRunSpeedMedium;

            case 2:
                return attackRunSpeedHard;

            default:
                return attackRunSpeedEasy;
        }
    }

    public void RunToAlarmPoint()
    {
        // Start running to the alarm point.
        if (AlarmSystem.Instance == null || AlarmSystem.Instance.AlarmPoint == null)
        {
            Debug.LogWarning($"{name}: No AlarmSystem or AlarmPoint assigned.");
            return;
        }

        isRunningToAlarmPoint = true;
        hasTriggeredMasterAlarm = false;
        isInvestigating = false;
        hasReachedInvestigationPoint = false;
        investigationLookPhase = 0;
        isReturningHome = false;
        suspicionActive = false;
        isWaiting = false;
        isSearching = false;

        agent.ResetPath();
        agent.speed = alarmRunSpeed;
        agent.SetDestination(AlarmSystem.Instance.AlarmPoint.position);
    }

    public void RunToSearchPosition(Vector3 searchPosition)
    {
        // Start or update a combat/search movement target.
        if (agent == null || !agent.enabled || !agent.isOnNavMesh)
            return;

        bool isNewSearch = !isSearching;
        bool targetChanged = Vector3.Distance(currentSearchCenter, searchPosition) > searchUpdateThreshold;

        isSearching = true;
        isRunningToAlarmPoint = false;
        isInvestigating = false;
        hasReachedInvestigationPoint = false;
        investigationLookPhase = 0;
        isReturningHome = false;
        suspicionActive = false;
        isWaiting = false;

        if (isNewSearch || targetChanged)
        {
            currentSearchCenter = searchPosition;
            currentSearchPosition = searchPosition;

            agent.ResetPath();
            agent.isStopped = false;
            agent.speed = GetAttackRunSpeedByDifficulty();
            agent.SetDestination(currentSearchPosition);

            if (debugLogSearch)
                Debug.Log($"{name}: Search destination updated -> {currentSearchPosition}");
        }
    }

    public void StopSearch()
    {
        // Stop the current search state.
        if (!isSearching)
            return;

        isSearching = false;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        if (debugLogSearch)
            Debug.Log($"{name}: Search stopped.");
    }
}