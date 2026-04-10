using UnityEngine;

public class EnemieBrain : MonoBehaviour
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

    [Header("Reaction Settings")]
    [SerializeField] private AlertReactionMode reactionMode = AlertReactionMode.AlarmThenRunToAlarmPoint;
    [SerializeField] private bool debugLogReaction = true;

    [Header("Suspicion Rotation")]
    [SerializeField] private float turnDelay = 0.9f;
    [SerializeField] private float fastTurnDelay = 0.2f;
    [SerializeField] private float turnSpeed = 5f;

    [Header("Suspicion Reset")]
    [SerializeField] private float suspicionResetTime = 7f;

    private EnemyVoice voice;

    private const int AlarmVoiceIndex = 1;
    private const int SuspicionVoiceIndex = 0;
    private const int SuspicionLevel2VoiceIndex = 2;

    private bool hasReactedToDetection = false;
    private bool isProvoked = false;

    // Suspicion Level
    private int suspicionLevel = 0;
    private float lastSuspicionTime = 0f;

    // Voice once-per-situation
    private bool hasPlayedSuspicionVoice = false;
    private bool hasPlayedSuspicionLevel2Voice = false;
    private bool hasPlayedAlarmVoice = false;

    // Rotation
    private bool isTurningToSuspicion = false;
    private float turnTimer = 0f;
    private Vector3 targetDirection;

    private void Awake()
    {
        if (detection == null)
            detection = GetComponent<EnemyDetection>();

        if (patrol == null)
            patrol = GetComponent<EnemyNavMeshPatrol>();

        if (voice == null)
            voice = GetComponent<EnemyVoice>();
    }

    private void Reset()
    {
        detection = GetComponent<EnemyDetection>();
        patrol = GetComponent<EnemyNavMeshPatrol>();
        voice = GetComponent<EnemyVoice>();
    }

    private void Update()
    {
        HandleSuspicionTurn();
        HandleSuspicionReset();

        if (detection == null)
            return;

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

    public void OnSuspicionStarted()
    {
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

                // Aktuell gleiche Reaktionszeit für beide Phasen gewünscht:
                // turnTimer = turnDelay;

                // Falls du später Phase 2 wieder schneller willst:
                turnTimer = (suspicionLevel == 1) ? turnDelay : fastTurnDelay;
            }
        }
    }

    private void HandleSuspicionTurn()
    {
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

    private void HandleAlarmThenRun()
    {
        if (!detection.CanSeePlayer)
            return;

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

    private void HandleAttackImmediately()
    {
        if (!detection.CanSeePlayer)
            return;

        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AttackImmediately");
    }

    private void HandleAttackIfProvoked()
    {
        if (!isProvoked)
            return;

        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AttackIfProvoked");
    }

    public void SetProvoked(bool value)
    {
        isProvoked = value;
    }
}
