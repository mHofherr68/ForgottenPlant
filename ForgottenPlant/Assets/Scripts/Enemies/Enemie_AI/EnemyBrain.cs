using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyBrain : MonoBehaviour
{
    // Defines how the enemy should behave once combat starts.
    public enum AlertReactionMode
    {
        AttackImmediately,
        AttackIfProvoked
    }

    [Header("References")]
    // Handles visual detection and stores the last known player position.
    [SerializeField] private EnemyDetection detection;

    // Controls movement, patroling, alarm running and search navigation.
    [SerializeField] private EnemyNavMeshPatrol patrol;

    // Reference to the player controller used for movement and position checks.
    [SerializeField] private PlayerController playerController;

    // Optional weapon controller used by armed enemies.
    [SerializeField] private EnemyWeaponController weaponController;

    [Header("Reaction Settings")]
    // Defines which combat behavior this enemy should use.
    [SerializeField] private AlertReactionMode reactionMode = AlertReactionMode.AttackImmediately;

    // Enables debug output for reaction and combat state changes.
    [SerializeField] private bool debugLogReaction = true;

    [Header("Suspicion Rotation")]
    // Delay before the enemy starts turning during first suspicion.
    [SerializeField] private float turnDelay = 0.9f;

    // Faster delay used for stronger suspicion states.
    [SerializeField] private float fastTurnDelay = 0.2f;

    // Rotation speed used for suspicion and general turning.
    [SerializeField] private float turnSpeed = 5f;

    [Header("Rear Search")]
    // Maximum distance at which the player can trigger a rear search.
    [SerializeField] private float rearTriggerDistance = 4f;

    // Rear trigger angle used to detect the player behind the enemy.
    [SerializeField, Range(0f, 180f)] private float rearTriggerAngle = 120f;

    // Cooldown before another rear search can be triggered.
    [SerializeField] private float rearSearchCooldown = 2.5f;

    // Short startled delay before the enemy actually starts turning.
    [SerializeField] private float schreckSekunde = 0.35f;

    // Enables debug output for rear search behavior.
    [SerializeField] private bool debugLogRearSearch = true;

    [Header("Suspicion Reset")]
    // Time after which suspicion is reset if nothing else happens.
    [SerializeField] private float suspicionResetTime = 7f;

    // Optional voice component used for enemy voice lines.
    private EnemyVoice voice;

    // NavMesh agent used for movement related stop/resume logic.
    private NavMeshAgent agent;

    // Voice indices used by the enemy voice system.
    private const int AlarmVoiceIndex = 1;
    private const int SuspicionVoiceIndex = 0;
    private const int SuspicionLevel2VoiceIndex = 2;
    private const int RearSearchVoiceIndex = 3;

    // Global list of all active enemies used for group calls.
    private static readonly List<EnemyBrain> allEnemies = new List<EnemyBrain>();

    // True once the enemy has reacted to detection or alarm logic.
    private bool hasReactedToDetection = false;

    // Used by the "AttackIfProvoked" mode.
    private bool isProvoked = false;

    // True once the enemy has entered the post-alarm combat phase.
    private bool hasEnteredAttackMode = false;

    // Current suspicion level.
    private int suspicionLevel = 0;

    // Timestamp of the last suspicion event.
    private float lastSuspicionTime = 0f;

    // Prevents repeating the same voice lines too often.
    private bool hasPlayedSuspicionVoice = false;
    private bool hasPlayedSuspicionLevel2Voice = false;
    private bool hasPlayedAlarmVoice = false;

    // Controls turning toward the last suspicious direction.
    private bool isTurningToSuspicion = false;
    private float turnTimer = 0f;
    private Vector3 targetDirection;

    // Rear search state data.
    private bool isRearSearching = false;
    private bool isRearSearchWaiting = false;
    private float rearSearchWaitTimer = 0f;
    private float rearSearchRemainingAngle = 0f;
    private float lastRearSearchTime = -999f;
    private bool rearSearchStoppedAgent = false;

    // Counts heard gunshots before escalation.
    private int heardGunshotCount = 0;

    // Tracks whether the agent was stopped specifically by gunshot reaction.
    private bool isStoppedByGunshot = false;

    private void Awake()
    {
        // Register this enemy in the global list.
        if (!allEnemies.Contains(this))
            allEnemies.Add(this);

        // Auto-assign missing references from the same GameObject.
        if (detection == null)
            detection = GetComponent<EnemyDetection>();

        if (patrol == null)
            patrol = GetComponent<EnemyNavMeshPatrol>();

        if (voice == null)
            voice = GetComponent<EnemyVoice>();

        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        // Find the player and weapon automatically if not assigned.
        if (playerController == null)
            playerController = FindFirstObjectByType<PlayerController>();

        if (weaponController == null)
            weaponController = GetComponentInChildren<EnemyWeaponController>();
    }

    private void OnDestroy()
    {
        // Remove this enemy from the global list when destroyed.
        allEnemies.Remove(this);
    }

    private void Reset()
    {
        // Rebuild all main references when Reset is used in the Inspector.
        detection = GetComponent<EnemyDetection>();
        patrol = GetComponent<EnemyNavMeshPatrol>();
        voice = GetComponent<EnemyVoice>();
        agent = GetComponent<NavMeshAgent>();
        playerController = FindFirstObjectByType<PlayerController>();
        weaponController = GetComponentInChildren<EnemyWeaponController>();
    }

    private void Update()
    {
        // Continuously process suspicion turning, rear search and suspicion reset.
        HandleSuspicionTurn();
        HandleRearSearch();
        HandleSuspicionReset();

        if (detection == null)
            return;

        // Once the global alarm is active, switch into the chosen combat mode.
        if (AlarmSystem.Instance != null && AlarmSystem.Instance.IsAlarmActive && !hasEnteredAttackMode)
        {
            EnterChosenAttackMode();
            return;
        }

        // Process post-alarm attack behavior.
        if (hasEnteredAttackMode)
        {
            switch (reactionMode)
            {
                case AlertReactionMode.AttackImmediately:
                    HandleAttackModeImmediately();
                    break;

                case AlertReactionMode.AttackIfProvoked:
                    HandleAttackModeIfProvoked();
                    break;
            }

            return;
        }

        // Rear search only exists before the enemy has fully reacted.
        if (!hasReactedToDetection)
        {
            TryStartRearSearch();
        }

        if (hasReactedToDetection)
            return;

        // If the player is directly seen, start alarm chain behavior.
        HandleAlarmThenRun();

        if (hasReactedToDetection)
            return;

        // Before full alarm, process the selected initial reaction mode.
        switch (reactionMode)
        {
            case AlertReactionMode.AttackImmediately:
                HandleAttackImmediately();
                break;

            case AlertReactionMode.AttackIfProvoked:
                HandleAttackIfProvoked();
                break;
        }
    }

    public static void NotifyGunshot(Vector3 gunshotPosition)
    {
        // Notify all active enemies that a gunshot was heard.
        for (int i = 0; i < allEnemies.Count; i++)
        {
            EnemyBrain enemy = allEnemies[i];

            if (enemy == null)
                continue;

            enemy.OnGunshotHeard(gunshotPosition);
        }
    }

    public static void NotifyNearImpact(Vector3 impactPosition, float radius)
    {
        // Notify all enemies within the impact radius.
        float sqrRadius = radius * radius;

        for (int i = 0; i < allEnemies.Count; i++)
        {
            EnemyBrain enemy = allEnemies[i];

            if (enemy == null)
                continue;

            Vector3 toImpact = enemy.transform.position - impactPosition;
            toImpact.y = 0f;

            if (toImpact.sqrMagnitude > sqrRadius)
                continue;

            enemy.OnNearImpactHeard(impactPosition);
        }
    }

    public void SetPlayerController(PlayerController controller)
    {
        // Allows assigning the player controller externally.
        playerController = controller;
    }

    public void OnDirectHit(Vector3 hitPosition)
    {
        // Direct hits immediately alert all enemies to search the target area.
        Vector3 searchTarget = GetCurrentSearchTargetPosition();

        if (searchTarget == transform.position && hitPosition != Vector3.zero)
            searchTarget = hitPosition;

        AlertAllEnemiesToSearchPosition(searchTarget);

        if (debugLogReaction)
            Debug.Log($"{name}: Direct hit -> alert all enemies to search at {searchTarget}");
    }

    public void OnSuspicionStarted()
    {
        // Do not start suspicion turning while rear search is active.
        if (isRearSearching || isRearSearchWaiting)
            return;

        // Increase suspicion level, capped at level 3.
        suspicionLevel++;

        if (suspicionLevel > 3)
            suspicionLevel = 3;

        lastSuspicionTime = Time.time;

        // At level 3, immediately escalate to alarm.
        if (suspicionLevel >= 3)
        {
            if (debugLogReaction)
                Debug.Log($"{name}: Suspicion level 3 reached -> immediate alarm.");

            ForceImmediateAlarm();
            return;
        }

        // Play voice lines depending on suspicion strength.
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

        // Turn toward the last known suspicious player position.
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
        // Ignore rear search once the enemy has fully reacted.
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

    private void OnGunshotHeard(Vector3 gunshotPosition)
    {
        // If already in attack mode, convert the event into a group search update.
        if (hasEnteredAttackMode || (AlarmSystem.Instance != null && AlarmSystem.Instance.IsAlarmActive))
        {
            AlertAllEnemiesToSearchPosition(GetCurrentSearchTargetPosition());
            return;
        }

        if (hasReactedToDetection)
            return;

        StopRearSearch(false);

        // Multiple gunshots escalate to immediate alarm.
        heardGunshotCount++;

        if (heardGunshotCount >= 2)
        {
            if (debugLogReaction)
                Debug.Log($"{name}: Multiple gunshots heard -> RunToAlarmPoint");

            ForceImmediateAlarm();
            return;
        }

        // First gunshot starts suspicion state.
        suspicionLevel = 1;
        lastSuspicionTime = Time.time;

        if (voice != null && !hasPlayedSuspicionVoice)
        {
            voice.PlayVoice(SuspicionVoiceIndex);
            hasPlayedSuspicionVoice = true;
        }

        // Turn toward the gunshot source.
        Vector3 direction = gunshotPosition - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            targetDirection = direction.normalized;
            isTurningToSuspicion = true;
            turnTimer = turnDelay;
        }

        // Stop movement while reacting to the gunshot.
        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            isStoppedByGunshot = true;
        }

        if (debugLogReaction)
            Debug.Log($"{name}: Gunshot heard -> Suspicion turn.");
    }

    private void OnNearImpactHeard(Vector3 impactPosition)
    {
        // If already in attack mode, treat nearby impacts as a search update.
        if (hasEnteredAttackMode || (AlarmSystem.Instance != null && AlarmSystem.Instance.IsAlarmActive))
        {
            AlertAllEnemiesToSearchPosition(GetCurrentSearchTargetPosition());
            return;
        }

        if (hasReactedToDetection)
            return;

        if (debugLogReaction)
            Debug.Log($"{name}: Near impact heard -> RunToAlarmPoint");

        // Nearby bullet impacts immediately escalate to alarm behavior.
        ForceImmediateAlarm();
    }

    private void HandleSuspicionTurn()
    {
        if (isRearSearching || isRearSearchWaiting)
            return;

        if (!isTurningToSuspicion)
            return;

        // Wait before starting the turn.
        turnTimer -= Time.deltaTime;
        if (turnTimer > 0f)
            return;

        // Smoothly turn toward the suspicious direction.
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

        // Reset suspicion after enough quiet time has passed.
        if (Time.time - lastSuspicionTime > suspicionResetTime)
        {
            suspicionLevel = 0;
            heardGunshotCount = 0;

            hasPlayedSuspicionVoice = false;
            hasPlayedSuspicionLevel2Voice = false;
            hasPlayedAlarmVoice = false;

            isTurningToSuspicion = false;

            // Resume movement if it had been stopped by gunshot reaction.
            if (isStoppedByGunshot && agent != null && agent.enabled && agent.isOnNavMesh)
            {
                agent.isStopped = false;
                isStoppedByGunshot = false;
            }
        }
    }

    private void TryStartRearSearch()
    {
        // Rear search is only available while no stronger reaction is active.
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

        // Rear search only triggers if the player is moving and not sneaking/crouching.
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

        // Only trigger if the player is within the configured rear angle.
        float angleToPlayer = Vector3.Angle(transform.forward, toPlayer.normalized);
        float rearHalfAngle = rearTriggerAngle * 0.5f;
        float rearMinAngle = 180f - rearHalfAngle;

        if (angleToPlayer < rearMinAngle)
            return;

        StartRearSearch();
    }

    private void StartRearSearch()
    {
        // Start rear search with an initial waiting phase.
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
            // Abort rear search if the player becomes visible.
            if (detection != null && detection.CanSeePlayer)
            {
                StopRearSearch(false);

                if (debugLogRearSearch)
                    Debug.Log($"{name}: Rear wait stopped - player spotted.");

                return;
            }

            // Count down the startled delay before turning begins.
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

        // Stop rear search if the player becomes visible while turning.
        if (detection != null && detection.CanSeePlayer)
        {
            StopRearSearch(false);

            if (debugLogRearSearch)
                Debug.Log($"{name}: Rear search stopped - player spotted.");

            return;
        }

        // Rotate through a full 360 degree search.
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
        // Clear all rear search state flags.
        isRearSearchWaiting = false;
        isRearSearching = false;
        rearSearchWaitTimer = 0f;

        if (resumeAgent)
            ResumeAgentAfterRearSearch();
    }

    private void StopAgentForRearSearch()
    {
        // Stop the NavMesh agent during rear search if possible.
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
        // Resume the NavMesh agent only if this script had stopped it.
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
        // The rear search system only cares about meaningful player movement.
        if (playerController == null)
            return false;

        return playerController.MoveInput.sqrMagnitude > 0.01f;
    }

    private void HandleAlarmThenRun()
    {
        // Once the player is seen, begin the alarm chain.
        if (!detection.CanSeePlayer)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: AlarmThenRunToAlarmPoint");

        if (voice != null && !hasPlayedAlarmVoice)
        {
            voice.PlayVoice(AlarmVoiceIndex);
            hasPlayedAlarmVoice = true;
        }

        AlertAllEnemiesToRunToAlarmPoint();
    }

    public void ForceImmediateAlarm()
    {
        // Prevent repeating the same escalation.
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

        AlertAllEnemiesToRunToAlarmPoint();
    }

    private void HandleAttackImmediately()
    {
        // In this mode, direct visibility instantly transitions the enemy into attack behavior.
        if (!detection.CanSeePlayer)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AttackImmediately");
    }

    private void HandleAttackIfProvoked()
    {
        // This mode only reacts fully once the enemy has been provoked.
        if (!isProvoked)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AttackIfProvoked");
    }

    private void HandleAttackModeImmediately()
    {
        // Aggressive combat mode: armed enemies shoot, unarmed enemies chase.
        if (detection.CanSeePlayer)
        {
            if (weaponController != null)
            {
                if (patrol != null)
                    patrol.StopSearch();

                HandleCombatAttackImmediately();
            }
            else
            {
                HandleCombatFollowVisiblePlayer();
            }
        }
        else
        {
            HandleCombatSearch();
        }
    }

    private void HandleAttackModeIfProvoked()
    {
        // More controlled combat mode: keep facing the player, fire if armed, otherwise hold/search.
        if (detection.CanSeePlayer)
        {
            if (patrol != null)
                patrol.StopSearch();

            if (weaponController != null)
            {
                HandleCombatAttackImmediately();
            }
            else
            {
                Vector3 direction = playerController != null
                    ? playerController.transform.position - transform.position
                    : Vector3.zero;

                direction.y = 0f;

                if (direction.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
                }
            }
        }
        else
        {
            HandleCombatSearch();
        }
    }

    private void HandleCombatAttackImmediately()
    {
        // Armed combat behavior: rotate toward the player and fire once aligned.
        if (playerController == null)
            return;

        Vector3 direction = playerController.transform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }

        if (weaponController == null)
            return;

        weaponController.SetTarget(playerController.transform);

        Quaternion aimRotation = Quaternion.LookRotation(direction.normalized);
        float angle = Quaternion.Angle(transform.rotation, aimRotation);
        if (angle > 8f)
            return;

        weaponController.TryFire();
    }

    private void HandleCombatFollowVisiblePlayer()
    {
        // Unarmed aggressive behavior: follow the player's current position.
        if (playerController == null)
            return;

        StartSearchToPosition(playerController.transform.position);
    }

    private void HandleCombatSearch()
    {
        // Search around the last known player position when the player is not visible.
        StartSearchToPosition(GetCurrentSearchTargetPosition());
    }

    private void StartSearchToPosition(Vector3 searchPosition)
    {
        // Forward the search target to the navigation component.
        if (patrol == null)
            return;

        patrol.RunToSearchPosition(searchPosition);

        if (debugLogReaction)
            Debug.Log($"{name}: Search -> {searchPosition}");
    }

    private Vector3 GetCurrentSearchTargetPosition()
    {
        // Prefer the last known player position from the detection system.
        if (detection != null && detection.LastKnownPlayerPosition != Vector3.zero)
            return detection.LastKnownPlayerPosition;

        // Fallback to the player's current position if available.
        if (playerController != null)
            return playerController.transform.position;

        // Final fallback: this enemy's own position.
        return transform.position;
    }

    private void AlertAllEnemiesToRunToAlarmPoint()
    {
        // Inform all enemies that they should run to the alarm point.
        for (int i = 0; i < allEnemies.Count; i++)
        {
            EnemyBrain otherEnemy = allEnemies[i];

            if (otherEnemy == null)
                continue;

            otherEnemy.RunToAlarmPointByCall();
        }
    }

    private void AlertAllEnemiesToSearchPosition(Vector3 searchPosition)
    {
        // Ensure the global alarm is active before group search begins.
        if (AlarmSystem.Instance != null && !AlarmSystem.Instance.IsAlarmActive)
        {
            AlarmSystem.Instance.TriggerAlarm();
        }

        // Tell every enemy to enter search mode at the same target position.
        for (int i = 0; i < allEnemies.Count; i++)
        {
            EnemyBrain otherEnemy = allEnemies[i];

            if (otherEnemy == null)
                continue;

            otherEnemy.ReceiveSearchCall(searchPosition);
        }
    }

    private void RunToAlarmPointByCall()
    {
        // Called when another enemy triggered the alarm.
        StopRearSearch(false);
        hasReactedToDetection = true;

        if (voice != null && !hasPlayedAlarmVoice)
        {
            voice.PlayVoice(4);
            hasPlayedAlarmVoice = true;
        }

        if (patrol != null)
            patrol.RunToAlarmPoint();
    }

    private void ReceiveSearchCall(Vector3 searchPosition)
    {
        // Called when another enemy alerts this one to search.
        StopRearSearch(false);
        hasReactedToDetection = true;
        hasEnteredAttackMode = true;

        if (voice != null && !hasPlayedAlarmVoice)
        {
            voice.PlayVoice(4);
            hasPlayedAlarmVoice = true;
        }

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = false;
        }

        StartSearchToPosition(searchPosition);

        if (debugLogReaction)
            Debug.Log($"{name}: Received search call -> {searchPosition}");
    }

    private void EnterChosenAttackMode()
    {
        // Enter the post-alarm attack state only once.
        if (hasEnteredAttackMode)
            return;

        StopRearSearch(false);
        hasReactedToDetection = true;
        hasEnteredAttackMode = true;

        if (agent != null && agent.enabled && agent.isOnNavMesh)
        {
            agent.ResetPath();
            agent.isStopped = true;
        }

        // Immediately start searching the current target position.
        StartSearchToPosition(GetCurrentSearchTargetPosition());

        switch (reactionMode)
        {
            case AlertReactionMode.AttackImmediately:
                if (debugLogReaction)
                    Debug.Log($"{name}: Reaction = AttackImmediately");
                break;

            case AlertReactionMode.AttackIfProvoked:
                if (debugLogReaction)
                    Debug.Log($"{name}: Reaction = AttackIfProvoked");
                break;
        }
    }

    public void SetProvoked(bool value)
    {
        // External setter for the provoked state.
        isProvoked = value;
    }
}