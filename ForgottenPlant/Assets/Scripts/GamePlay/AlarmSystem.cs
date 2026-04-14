using System.Collections;
using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
    public static AlarmSystem Instance { get; private set; }

    [Header("Alarm Target")]
    [SerializeField] private Transform alarmPoint;

    [Header("Alarm Effects")]
    [SerializeField] private AlarmLamp alarmLamp;
    [SerializeField] private FenceGateController entranceGate;

    [Header("Timing")]
    [SerializeField] private float gateDelay = 2f;

    [Header("Debug")]
    [SerializeField] private bool debugLog = true;

    public bool IsAlarmActive { get; private set; }
    public Transform AlarmPoint => alarmPoint;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TriggerAlarm()
    {
        if (IsAlarmActive)
            return;

        IsAlarmActive = true;

        if (debugLog)
            Debug.Log("MASTER ALARM TRIGGERED!");

        StartCoroutine(AlarmRoutine());
    }

    private IEnumerator AlarmRoutine()
    {
        ActivateAlarmLamp();

        if (gateDelay > 0f)
            yield return new WaitForSeconds(gateDelay);

        if (entranceGate != null)
        {
            entranceGate.TriggerGate();

            if (debugLog)
                Debug.Log("AlarmSystem: Entrance gate triggered.");
        }
    }

    private void ActivateAlarmLamp()
    {
        if (alarmLamp == null)
            return;

        alarmLamp.ActivateAlarmLamp();

        if (debugLog)
            Debug.Log("AlarmSystem: Alarm lamp activated.");
    }
}