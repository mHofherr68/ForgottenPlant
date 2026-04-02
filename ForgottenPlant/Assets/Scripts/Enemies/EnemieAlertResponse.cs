using UnityEngine;

public class EnemyAlertResponse : MonoBehaviour
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

    private bool hasReactedToDetection = false;
    private bool isProvoked = false;

    private void Reset()
    {
        detection = GetComponent<EnemyDetection>();
        patrol = GetComponent<EnemyNavMeshPatrol>();
    }

    private void Update()
    {
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

    /*private void HandleAlarmThenRun()
    {
        if (!detection.CanSeePlayer)
            return;

        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AlarmThenRunToAlarmPoint");

        AlarmSystem.Instance?.TriggerAlarm();

        if (patrol != null)
        {
            patrol.RunToAlarmPoint();
        }
    }*/

    private void HandleAlarmThenRun()
    {
        if (!detection.CanSeePlayer)
            return;

        hasReactedToDetection = true;

        AlarmSystem.Instance?.TriggerAlarm();

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

        // V1: erstmal nur Log
        // Später hier Combat / Chase starten
    }

    private void HandleAttackIfProvoked()
    {
        if (!isProvoked)
            return;

        hasReactedToDetection = true;

        if (debugLogReaction)
            Debug.Log($"{name}: Reaction = AttackIfProvoked");

        // V1: erstmal nur Log
        // Später hier Combat / Chase starten
    }

    public void SetProvoked(bool value)
    {
        isProvoked = value;
    }
}
