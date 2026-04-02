using UnityEngine;

public class AlarmLamp : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Light alarmLight;
    [SerializeField] private AudioSource alarmAudioSource;

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Timing")]
    [SerializeField] private float activeDuration = 6f;

    [Header("Test")]
    [SerializeField] private bool testActivate = false;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private bool isActive = false;
    private float activeTimer = 0f;

    private bool lastTestState = false;

    private void Awake()
    {
        SetActiveState(false);
    }

    private void Update()
    {
        // 🧪 TEST TRIGGER (nur wenn Wert sich ändert)
        if (testActivate && !lastTestState)
        {
            ActivateAlarmLamp();
        }

        lastTestState = testActivate;

        if (!isActive)
            return;

        activeTimer -= Time.deltaTime;

        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);

        if (activeTimer <= 0f)
        {
            DeactivateAlarmLamp();
            testActivate = false; // Reset im Inspector
        }
    }

    public void ActivateAlarmLamp()
    {
        if (debugLog)
            Debug.Log($"{name}: Alarm lamp activated.");

        isActive = true;
        activeTimer = activeDuration;

        if (alarmLight != null)
        {
            alarmLight.enabled = true;
        }

        if (alarmAudioSource != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Play();
        }
    }

    public void DeactivateAlarmLamp()
    {
        if (debugLog)
            Debug.Log($"{name}: Alarm lamp deactivated.");

        SetActiveState(false);
    }

    private void SetActiveState(bool active)
    {
        isActive = active;
        activeTimer = 0f;

        if (alarmLight != null)
        {
            alarmLight.enabled = active;
        }

        if (alarmAudioSource != null)
        {
            if (active)
            {
                if (!alarmAudioSource.isPlaying)
                    alarmAudioSource.Play();
            }
            else
            {
                alarmAudioSource.Stop();
            }
        }
    }
}
