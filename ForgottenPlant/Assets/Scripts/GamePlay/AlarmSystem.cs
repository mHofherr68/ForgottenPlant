using System.Collections;
using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
    // Global singleton instance used to access the alarm system from other scripts.
    public static AlarmSystem Instance { get; private set; }

    [Header("Alarm Target")]
    // Target position all enemies should use when running to the alarm point.
    [SerializeField] private Transform alarmPoint;

    [Header("Alarm Effects")]
    // Alarm lamp that will be activated once the master alarm starts.
    [SerializeField] private AlarmLamp alarmLamp;

    // Entrance gate that will be triggered after a short delay.
    [SerializeField] private FenceGateController entranceGate;

    [Header("Timing")]
    // Delay between alarm activation and gate trigger.
    [SerializeField] private float gateDelay = 2f;

    [Header("Debug")]
    // Enables debug logs for alarm events.
    [SerializeField] private bool debugLog = true;

    // True once the master alarm has been activated.
    public bool IsAlarmActive { get; private set; }

    // Public read-only access to the alarm point.
    public Transform AlarmPoint => alarmPoint;

    private void Awake()
    {
        // Enforce singleton behavior so only one alarm system exists in the scene.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerAlarm()
    {
        // Prevent the alarm from being triggered more than once.
        if (IsAlarmActive)
            return;

        IsAlarmActive = true;

        if (debugLog)
            Debug.Log("MASTER ALARM TRIGGERED!");

        // Start the full alarm sequence.
        StartCoroutine(AlarmRoutine());
    }

    private IEnumerator AlarmRoutine()
    {
        // Activate visual alarm feedback immediately.
        ActivateAlarmLamp();

        // Wait before opening the entrance gate, if a delay is configured.
        if (gateDelay > 0f)
            yield return new WaitForSeconds(gateDelay);

        // Trigger the gate once the delay has passed.
        if (entranceGate != null)
        {
            entranceGate.TriggerGate();

            if (debugLog)
                Debug.Log("AlarmSystem: Entrance gate triggered.");
        }
    }

    private void ActivateAlarmLamp()
    {
        // Stop if no alarm lamp is assigned.
        if (alarmLamp == null)
            return;

        // Turn on the alarm lamp effect.
        alarmLamp.ActivateAlarmLamp();

        if (debugLog)
            Debug.Log("AlarmSystem: Alarm lamp activated.");
    }
}