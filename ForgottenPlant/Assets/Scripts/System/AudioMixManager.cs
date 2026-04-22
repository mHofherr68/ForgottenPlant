using UnityEngine;
using UnityEngine.Audio;

public class AudioMixManager : MonoBehaviour
{
    [Header("Mixer")]
    // Main AudioMixer used to control all exposed volume parameters.
    [SerializeField] private AudioMixer mainAudioMixer;

    [Header("Music")]
    // AudioSource used for background music playback.
    [SerializeField] private AudioSource musicSource;

    // List of available music tracks that can be selected through the settings system.
    [SerializeField] private AudioClip[] tracks;

    // Stores the currently active track index to avoid unnecessary restarts.
    private int currentTrackIndex = -1;

    private void Awake()
    {
        // Auto-assign the AudioSource on this GameObject if none was set manually.
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
    }

    public void ApplySettings(GameRuntimeSettings settings)
    {
        // Stop if no settings object was provided.
        if (settings == null)
            return;

        // Apply the selected music track and the mixer volume values.
        ApplyMusicTrack(settings);
        ApplyMixerVolumes(settings);
    }

    private void ApplyMusicTrack(GameRuntimeSettings settings)
    {
        // Stop if music playback is not properly configured.
        if (musicSource == null || tracks == null || tracks.Length == 0)
            return;

        // Clamp the selected track index to a valid range.
        int trackIndex = Mathf.Clamp(settings.trackIndex, 0, tracks.Length - 1);

        // Only switch tracks if the selected one is different from the currently active one.
        if (currentTrackIndex != trackIndex || musicSource.clip != tracks[trackIndex])
        {
            currentTrackIndex = trackIndex;
            musicSource.clip = tracks[trackIndex];
            musicSource.Play();
        }
    }

    private void ApplyMixerVolumes(GameRuntimeSettings settings)
    {
        // Stop if no AudioMixer is assigned.
        if (mainAudioMixer == null)
        {
            Debug.LogWarning("AudiomixerManager: No AudioMixer assigned.");
            return;
        }

        // Apply all supported volume categories to the exposed mixer parameters.
        SetMixerVolume("MasterVolume", settings.masterVolume);
        SetMixerVolume("MusicVolume", settings.musicVolume);
        SetMixerVolume("SFXVolume", settings.sfxVolume);
        SetMixerVolume("SpeechVolume", settings.speechVolume);
    }

    private void SetMixerVolume(string parameterName, float normalizedValue)
    {
        // Clamp the normalized value to avoid log(0).
        float clampedValue = Mathf.Clamp(normalizedValue, 0.0001f, 1f);

        // Convert normalized linear volume into decibel space.
        float volumeInDb = Mathf.Log10(clampedValue) * 20f;

        // Try to assign the value to the exposed mixer parameter.
        bool success = mainAudioMixer.SetFloat(parameterName, volumeInDb);

        if (!success)
        {
            Debug.LogWarning($"AudiomixerManager: Mixer parameter '{parameterName}' not found or not exposed.");
        }
    }
}