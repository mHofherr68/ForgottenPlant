/*using UnityEngine;

public class AlarmSystem : MonoBehaviour
{
    public static AlarmSystem Instance { get; private set; }

    public bool IsAlarmActive { get; private set; }

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
using UnityEngine;

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
}
