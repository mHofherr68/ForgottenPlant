using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    // Text element used to display the "Mission Failed" message.
    [SerializeField] private TMP_Text missionFailedText;

    // AudioMixer used to mute and restore game state audio during the game over scene.
    [SerializeField] private AudioMixer mainAudioMixer;

    [Header("Timing")]
    // Time the text stays fully visible before fading out.
    [SerializeField] private float visibleDuration = 2f;

    // Duration of the text fade-out animation.
    [SerializeField] private float fadeDuration = 1.5f;

    // Extra delay after the fade before loading the next scene.
    [SerializeField] private float loadDelay = 2f;

    [Header("Scene Loading")]
    // Scene that will be loaded after the game over sequence finishes.
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("Audio Mixer")]
    // Name of the exposed mixer parameter used to mute game state audio.
    [SerializeField] private string gameStateMuteParameter = "GSMVolume";

    // Mixer value used while muted.
    [SerializeField] private float mutedVolumeDb = -80f;

    // Mixer value used when restoring normal audio.
    [SerializeField] private float unmutedVolumeDb = 0f;

    [Header("Debug")]
    // Enables debug logs for the game over flow and audio state changes.
    [SerializeField] private bool debugLogFlow = false;

    // Tracks whether this script has already applied the audio mute.
    private bool hasAppliedMute = false;

    private void Start()
    {
        // Stop if no text reference is assigned.
        if (missionFailedText == null)
        {
            Debug.LogWarning($"{name}: No TMP_Text assigned.");
            return;
        }

        // Mute game state audio when entering the game over screen.
        ApplyGameStateMute();

        // Ensure the text starts fully visible.
        Color startColor = missionFailedText.color;
        startColor.a = 1f;
        missionFailedText.color = startColor;

        // Start the full game over presentation sequence.
        StartCoroutine(GameOverRoutine());
    }

    private void OnDisable()
    {
        // Restore mixer state if this object becomes disabled.
        ReleaseGameStateMute();
    }

    private void OnDestroy()
    {
        // Restore mixer state if this object is destroyed.
        ReleaseGameStateMute();
    }

    private IEnumerator GameOverRoutine()
    {
        if (debugLogFlow)
            Debug.Log($"{name}: Text visible for {visibleDuration} seconds.");

        // Keep the text fully visible for a short time.
        yield return new WaitForSeconds(visibleDuration);

        float elapsed = 0f;
        Color color = missionFailedText.color;

        // Fade the text alpha from fully visible to invisible.
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            color.a = Mathf.Lerp(1f, 0f, t);
            missionFailedText.color = color;

            yield return null;
        }

        // Snap the text to fully transparent at the end of the fade.
        color.a = 0f;
        missionFailedText.color = color;

        if (debugLogFlow)
            Debug.Log($"{name}: Text fade complete. Waiting {loadDelay} seconds before loading scene.");

        // Wait before loading the next scene.
        yield return new WaitForSeconds(loadDelay);

        // Stop if no valid next scene name is set.
        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning($"{name}: No next scene name assigned.");
            yield break;
        }

        if (debugLogFlow)
            Debug.Log($"{name}: Loading scene async -> {nextSceneName}");

        // Load the configured next scene asynchronously.
        SceneManager.LoadSceneAsync(nextSceneName);
    }

    private void ApplyGameStateMute()
    {
        // Stop if no AudioMixer is assigned.
        if (mainAudioMixer == null)
        {
            Debug.LogWarning($"{name}: No AudioMixer assigned.");
            return;
        }

        // Set the exposed mixer parameter to the muted value.
        mainAudioMixer.SetFloat(gameStateMuteParameter, mutedVolumeDb);
        hasAppliedMute = true;

        if (debugLogFlow)
        {
            if (mainAudioMixer.GetFloat(gameStateMuteParameter, out float currentValue))
                Debug.Log($"{name}: Set mixer parameter '{gameStateMuteParameter}' to {currentValue} dB.");
            else
                Debug.LogWarning($"{name}: Could not read mixer parameter '{gameStateMuteParameter}'.");
        }
    }

    private void ReleaseGameStateMute()
    {
        // Only restore audio if this script had muted it before.
        if (!hasAppliedMute)
            return;

        if (mainAudioMixer == null)
            return;

        // Restore the exposed mixer parameter to its unmuted value.
        mainAudioMixer.SetFloat(gameStateMuteParameter, unmutedVolumeDb);
        hasAppliedMute = false;

        if (debugLogFlow)
        {
            if (mainAudioMixer.GetFloat(gameStateMuteParameter, out float currentValue))
                Debug.Log($"{name}: Restored mixer parameter '{gameStateMuteParameter}' to {currentValue} dB.");
            else
                Debug.LogWarning($"{name}: Could not read mixer parameter '{gameStateMuteParameter}' after restore.");
        }
    }
}