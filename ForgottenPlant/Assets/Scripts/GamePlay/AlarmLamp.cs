using UnityEngine;

public class AlarmLamp : MonoBehaviour
{
    [Header("References")]
    // Light component used for the visual alarm effect.
    [SerializeField] private Light alarmLight;

    // Audio source used for the alarm sound.
    [SerializeField] private AudioSource alarmAudioSource;

    [Header("Rotation")]
    // Rotation speed of the alarm lamp while active.
    [SerializeField] private float rotationSpeed = 180f;

    [Header("Timing")]
    // Duration for which the alarm lamp stays active.
    [SerializeField] private float activeDuration = 6f;

    [Header("Test")]
    // Inspector test flag used to activate the lamp manually during runtime.
    [SerializeField] private bool testActivate = false;

    [Header("Debug")]
    // Enables debug logs for activation and deactivation events.
    [SerializeField] private bool debugLog = false;

    // True while the alarm lamp is active.
    private bool isActive = false;

    // Remaining active time countdown.
    private float activeTimer = 0f;

    // Stores the previous test flag state to detect rising edges.
    private bool lastTestState = false;

    private void Awake()
    {
        // Ensure the lamp starts in an inactive state.
        SetActiveState(false);
    }

    private void Update()
    {
        // Activate the alarm lamp once when the test flag is switched on.
        if (testActivate && !lastTestState)
        {
            ActivateAlarmLamp();
        }

        // Store the current test flag state for edge detection in the next frame.
        lastTestState = testActivate;

        // Stop update processing if the lamp is inactive.
        if (!isActive)
            return;

        // Decrease remaining active time.
        activeTimer -= Time.deltaTime;

        // Rotate the alarm lamp while it is active.
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.Self);

        // Deactivate the alarm lamp once the active duration has elapsed.
        if (activeTimer <= 0f)
        {
            DeactivateAlarmLamp();
            testActivate = false;
        }
    }

    public void ActivateAlarmLamp()
    {
        if (debugLog)
            Debug.Log($"{name}: Alarm lamp activated.");

        // Mark the lamp as active and reset the active timer.
        isActive = true;
        activeTimer = activeDuration;

        // Enable the alarm light.
        if (alarmLight != null)
        {
            alarmLight.enabled = true;
        }

        // Start the alarm sound if it is not already playing.
        if (alarmAudioSource != null && !alarmAudioSource.isPlaying)
        {
            alarmAudioSource.Play();
        }
    }

    public void DeactivateAlarmLamp()
    {
        if (debugLog)
            Debug.Log($"{name}: Alarm lamp deactivated.");

        // Reset the lamp to its inactive state.
        SetActiveState(false);
    }

    private void SetActiveState(bool active)
    {
        // Set the active flag and reset the timer.
        isActive = active;
        activeTimer = 0f;

        // Enable or disable the alarm light.
        if (alarmLight != null)
        {
            alarmLight.enabled = active;
        }

        // Start or stop the alarm audio depending on the active state.
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