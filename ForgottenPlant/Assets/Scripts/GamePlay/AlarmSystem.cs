/*using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
    public static AlarmSystem Instance { get; private set; }

    [Header("Alarm Target")]
    [SerializeField] private Transform alarmPoint;

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
        Debug.Log("ALARM TRIGGERED!");
    }
}*/
using System.Collections;
using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
    public static AlarmSystem Instance { get; private set; }

    [Header("Alarm Target")]
    [SerializeField] private Transform alarmPoint;

    [Header("Alarm Effects")]
    [SerializeField] private AlarmLamp[] alarmLamps;
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
        ActivateAlarmLamps();

        if (gateDelay > 0f)
            yield return new WaitForSeconds(gateDelay);

        if (entranceGate != null)
        {
            entranceGate.TriggerGate();

            if (debugLog)
                Debug.Log("AlarmSystem: Entrance gate triggered.");
        }
    }

    private void ActivateAlarmLamps()
    {
        if (alarmLamps == null || alarmLamps.Length == 0)
            return;

        foreach (AlarmLamp lamp in alarmLamps)
        {
            if (lamp == null)
                continue;

            lamp.ActivateAlarmLamp();
        }

        if (debugLog)
            Debug.Log("AlarmSystem: Alarm lamps activated.");
    }
}