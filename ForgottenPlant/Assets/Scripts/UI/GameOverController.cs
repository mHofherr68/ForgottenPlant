using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class GameOverController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text missionFailedText;
    [SerializeField] private AudioMixer mainAudioMixer;

    [Header("Timing")]
    [SerializeField] private float visibleDuration = 2f;
    [SerializeField] private float fadeDuration = 1.5f;
    [SerializeField] private float loadDelay = 2f;

    [Header("Scene Loading")]
    [SerializeField] private string nextSceneName = "MainMenu";

    [Header("Audio Mixer")]
    [SerializeField] private string gameStateMuteParameter = "GSMVolume";
    [SerializeField] private float mutedVolumeDb = -80f;
    [SerializeField] private float unmutedVolumeDb = 0f;

    [Header("Debug")]
    [SerializeField] private bool debugLogFlow = false;

    private bool hasAppliedMute = false;

    private void Start()
    {
        if (missionFailedText == null)
        {
            Debug.LogWarning($"{name}: No TMP_Text assigned.");
            return;
        }

        ApplyGameStateMute();

        Color startColor = missionFailedText.color;
        startColor.a = 1f;
        missionFailedText.color = startColor;

        StartCoroutine(GameOverRoutine());
    }

    private void OnDisable()
    {
        ReleaseGameStateMute();
    }

    private void OnDestroy()
    {
        ReleaseGameStateMute();
    }

    private IEnumerator GameOverRoutine()
    {
        if (debugLogFlow)
            Debug.Log($"{name}: Text visible for {visibleDuration} seconds.");

        yield return new WaitForSeconds(visibleDuration);

        float elapsed = 0f;
        Color color = missionFailedText.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);

            color.a = Mathf.Lerp(1f, 0f, t);
            missionFailedText.color = color;

            yield return null;
        }

        color.a = 0f;
        missionFailedText.color = color;

        if (debugLogFlow)
            Debug.Log($"{name}: Text fade complete. Waiting {loadDelay} seconds before loading scene.");

        yield return new WaitForSeconds(loadDelay);

        if (string.IsNullOrWhiteSpace(nextSceneName))
        {
            Debug.LogWarning($"{name}: No next scene name assigned.");
            yield break;
        }

        if (debugLogFlow)
            Debug.Log($"{name}: Loading scene async -> {nextSceneName}");

        SceneManager.LoadSceneAsync(nextSceneName);
    }

    private void ApplyGameStateMute()
    {
        if (mainAudioMixer == null)
        {
            Debug.LogWarning($"{name}: No AudioMixer assigned.");
            return;
        }

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
        if (!hasAppliedMute)
            return;

        if (mainAudioMixer == null)
            return;

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