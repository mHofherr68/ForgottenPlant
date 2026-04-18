using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    public enum AlertReactionMode
    {
        AlarmThenRunToAlarmPoint,
        AttackImmediately,
        AttackIfProvoked
    }

    [Header("References")]
    [SerializeField] private EnemyDetection detection;
    [SerializeField] private EnemyNavMeshPatrol patrol;
    [SerializeField] private PlayerController playerController;

    [Header("Reaction Settings")]
    [SerializeField] private AlertReactionMode reactionMode = AlertReactionMode.AlarmThenRunToAlarmPoint;
    [SerializeField] private bool debugLogReaction = true;

    [Header("Suspicion Rotation")]
    [SerializeField] private float turnDelay = 0.9f;
    [SerializeField] private float fastTurnDelay = 0.2f;
    [SerializeField] private float turnSpeed = 5f;

    [Header("Rear Search")]
    [SerializeField] private float rearTriggerDistance = 4f;
    [SerializeField, Range(0f, 180f)] private float rearTriggerAngle = 120f;
    [SerializeField] private float rearSearchCooldown = 2.5f;
    [SerializeField] private float schreckSekunde = 0.35f;
    [SerializeField] private bool debugLogRearSearch = true;

    [Header("Suspicion Reset")]
    [SerializeField] private float suspicionResetTime = 7f;

    private EnemyVoice voice;
    private NavMeshAgent agent;

    private const int AlarmVoiceIndex = 1;
    private const int SuspicionVoiceIndex = 0;
    private const int SuspicionLevel2VoiceIndex = 2;
    private const int RearSearchVoiceIndex = 3;

    private bool hasReactedToDetection = false;
    private bool isProvoked = false;

    private int suspicionLevel = 0;
    private float lastSuspicionTime = 0f;

    private bool hasPlayedSuspicionVoice = false;
    private bool hasPlayedSuspicionLevel2Voice = false;
    private bool hasPlayedAlarmVoice = false;

    private bool isTurningToSuspicion = false;
    private float turnTimer = 0f;
    private Vector3 targetDirection;

    private bool isRearSearching = false;
    private bool isRearSearchWaiting = false;
    private float rearSearchWaitTimer = 0f;
    private float rearSearchRemainingAngle = 0f;
    private float lastRearSearchTime = -999f;
    private bool rearSearchStoppedAgent = false;

    private void Awake()
    {
        if (detection == null)
            detection = GetComponent<EnemyDetection>();

        if (patrol == null)
            patrol = GetComponent<EnemyNavMeshPatrol>();

        if (voice == null)
            voice = GetComponent<EnemyVoice>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();
    }

    private void Reset()
    {
        detection = GetComponent<EnemyDetection>();
        patrol = GetComponent<EnemyNavMeshPatrol>();
        voice = GetComponent<EnemyVoice>();
        agent = GetComponent<NavMeshAgent>();
        playerController = FindFirstObjectByType<PlayerController>();
    }

    private void Update()
    {
        HandleSuspicionTurn();
        HandleRearSearch();
        HandleSuspicionReset();

        if (detection == null)
            return;

        if (!hasReactedToDetection)
        {
            TryStartRearSearch();
        }

        if (hasReactedToDetection)
            return;

        switch (reactionMode)
        {
            case AlertReactionMode.AlarmThenRunToAlarmPoint:
                HandleAlarmThenRun();
                break;

            case AlertReactionMode.AttackImmediately:
                HandleAttackImmediately();
                break;

            case AlertReactionMode.AttackIfProvoked:
                HandleAttackIfProvoked();
                break;
        }
    }

    public void SetPlayerController(PlayerController controller)
    {
        playerController = controller;
    }

    public void OnSuspicionStarted()
    {
        if (isRearSearching || isRearSearchWaiting)
            return;

        suspicionLevel++;

        if (suspicionLevel > 2)
            suspicionLevel = 2;

        lastSuspicionTime = Time.time;

        if (voice != null)
        {
            if (suspicionLevel == 1)
            {
                if (!hasPlayedSuspicionVoice)
                {
                    voice.PlayVoice(SuspicionVoiceIndex);
                    hasPlayedSuspicionVoice = true;
                }
            }
            else
            {
                if (!hasPlayedSuspicionLevel2Voice)
                {
                    voice.PlayVoice(SuspicionLevel2VoiceIndex);
                    hasPlayedSuspicionLevel2Voice = true;
                }
            }
        }

        if (detection != null)
        {
            Vector3 direction = detection.LastKnownPlayerPosition - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                targetDirection = direction.normalized;
                isTurningToSuspicion = true;
                turnTimer = (suspicionLevel == 1) ? turnDelay : fastTurnDelay;
            }
        }
    }

    public void TriggerRearSearchFromTouch()
    {
        if (hasReactedToDetection)
            return;

        if (isRearSearching || isRearSearchWaiting)
            return;

        if (isTurningToSuspicion)
            return;

        if (Time.time < lastRearSearchTime + rearSearchCooldown)
            return;

        StartRearSearch();

        if (debugLogRearSearch)
            Debug.Log($"{name}: Rear search triggered by touch.");
    }

    private void HandleSuspicionTurn()
    {
        if (isRearSearching || isRearSearchWaiting)
            return;

        if (!isTurningToSuspicion)
            return;

        turnTimer -= Time.deltaTime;
        if (turnTimer > 0f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

        float angle = Quaternion.Angle(transform.rotation, targetRotation);
        if (angle < 2f)
        {
            isTurningToSuspicion = false;
        }
    }

    private void HandleSuspicionReset()
    {
        if (suspicionLevel == 0)
            return;

        if (Time.time - lastSuspicionTime > suspicionResetTime)
        {
            suspicionLevel = 0;

            hasPlayedSuspicionVoice = false;
            hasPlayedSuspicionLevel2Voice = false;
            hasPlayedAlarmVoice = false;

            isTurningToSuspicion = false;
        }
    }

    private void TryStartRearSearch()
    {
        if (isRearSearching || isRearSearchWaiting)
            return;

        if (isTurningToSuspicion)
            return;

        if (Time.time < lastRearSearchTime + rearSearchCooldown)
            return;

        if (detection != null && (detection.CanSeePlayer || detection.HasSuspicion))
            return;

        if (playerController == null)
            return;

        Transform playerTransform = playerController.transform;
        if (playerTransform == null)
            return;

        if (!IsPlayerMoving())
            return;

        if (playerController.IsSneaking || playerController.IsCrouching)
            return;

        Vector3 toPlayer = playerTransform.position - transform.position;
        toPlayer.y = 0f;

        float distance = toPlayer.magnitude;
        if (distance > rearTriggerDistance)
            return;

        if (distance < 0.001f)
            return;

        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer.normalized);
        float rearHalfAngle = rearTriggerAngle * 0.5f;
        float rearMinAngle = 180f - rearHalfAngle;

        if (angleToPlayer < rearMinAngle)
            return;

        StartRearSearch();
    }

    private void StartRearSearch()
    {
        isRearSearchWaiting = true;
        isRearSearching = false;
        rearSearchWaitTimer = schreckSekunde;
        rearSearchRemainingAngle = 360f;
        lastRearSearchTime = Time.time;
        isTurningToSuspicion = false;

        StopAgentForRearSearch();

        if (voice != null)
        {
            voice.PlayVoice(RearSearchVoiceIndex);
        }

        if (debugLogRearSearch)
            Debug.Log($"{name}: Rear search triggered - startled, waiting before turn.");
    }

    private void HandleRearSearch()
    {
        if (isRearSearchWaiting)
        {
            if (detection != null && detection.CanSeePlayer)
            {
                StopRearSearch(false);

                if (debugLogRearSearch)
                    Debug.Log($"{name}: Rear wait stopped - player spotted.");

                return;
            }

            rearSearchWaitTimer -= Time.deltaTime;
            if (rearSearchWaitTimer <= 0f)
            {
                isRearSearchWaiting = false;
                isRearSearching = true;

                if (debugLogRearSearch)
                    Debug.Log($"{name}: Rear search started after schreckSekunde.");
            }

            return;
        }

        if (!isRearSearching)
            return;

        if (detection != null && detection.CanSeePlayer)
        {
            StopRearSearch(false);

            if (debugLogRearSearch)
                Debug.Log($"{name}: Rear search stopped - player spotted.");

            return;
        }

        float step = turnSpeed * 100f * Time.deltaTime;
        transform.Rotate(0f, step, 0f);

        rearSearchRemainingAngle -= step;
        if (rearSearchRemainingAngle <= 0f)
        {
            StopRearSearch(true);

            if (debugLogRearSearch)
                Debug.Log($"{name}: Rear search finished - no player found.");
        }
    }

    private void StopRearSearch(bool resumeAgent)
    {
        isRearSearchWaiting = false;
        isRearSearching = false;
        rearSearchWaitTimer = 0f;

        if (resumeAgent)
            ResumeAgentAfterRearSearch();
    }

    private void StopAgentForRearSearch()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            return;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            rearSearchStoppedAgent = true;
        }
    }

    private void ResumeAgentAfterRearSearch()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            return;

        if (!rearSearchStoppedAgent)
            return;

        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
        }

        rearSearchStoppedAgent = false;
    }

    private bool IsPlayerMoving()
    {
        if (playerController == null)
            return false;

        return playerController.MoveInput.sqrMagnitude > 0.01f;
    }

    private void HandleAlarmThenRun()
    {
        if (!detection.CanSeePlayer)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AlarmThenRunToAlarmPoint");

        if (voice != null && !hasPlayedAlarmVoice)
        {
            voice.PlayVoice(AlarmVoiceIndex);
            hasPlayedAlarmVoice = true;
        }

        if (patrol != null)
            patrol.RunToAlarmPoint();
    }

    public void ForceImmediateAlarm()
    {
        if (hasReactedToDetection)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: CONTACT -> IMMEDIATE ALARM");

        if (voice != null && !hasPlayedAlarmVoice)
        {
            voice.PlayVoice(AlarmVoiceIndex);
            hasPlayedAlarmVoice = true;
        }

        if (patrol != null)
            patrol.RunToAlarmPoint();
    }

    private void HandleAttackImmediately()
    {
        if (!detection.CanSeePlayer)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AttackImmediately");
    }

    private void HandleAttackIfProvoked()
    {
        if (!isProvoked)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AttackIfProvoked");
    }

    public void SetProvoked(bool value)
    {
        isProvoked = value;
    }
}