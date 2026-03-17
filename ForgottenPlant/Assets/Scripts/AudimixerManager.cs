/*using UnityEngine;

public class AudiomixerManager : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] tracks;

    private int currentTrackIndex = -1;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    public void ApplySettings(GameRuntimeSettings settings)
    {
        if (audioSource == null || tracks == null || tracks.Length == 0 || settings == null)
            return;

        int trackIndex = Mathf.Clamp(settings.trackIndex, 0, tracks.Length - 1);

        if (currentTrackIndex != trackIndex || audioSource.clip != tracks[trackIndex])
        {
            currentTrackIndex = trackIndex;
            audioSource.clip = tracks[trackIndex];
            audioSource.Play();
        }

        audioSource.volume = Mathf.Clamp01(settings.musicVolume);
    }
}*/
using UnityEngine;
using UnityEngine.Audio;

public class AudiomixerManager : MonoBehaviour
{
    [Header("Mixer")]
    [SerializeField] private AudioMixer mainAudioMixer;

    [Header("Music")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] tracks;

    private int currentTrackIndex = -1;

    private void Awake()
    {
        if (musicSource == null)
            musicSource = GetComponent<AudioSource>();
    }

    public void ApplySettings(GameRuntimeSettings settings)
    {
        if (settings == null)
            return;

        ApplyMusicTrack(settings);
        ApplyMixerVolumes(settings);
    }

    private void ApplyMusicTrack(GameRuntimeSettings settings)
    {
        if (musicSource == null || tracks == null || tracks.Length == 0)
            return;

        int trackIndex = Mathf.Clamp(settings.trackIndex, 0, tracks.Length - 1);

        if (currentTrackIndex != trackIndex || musicSource.clip != tracks[trackIndex])
        {
            currentTrackIndex = trackIndex;
            musicSource.clip = tracks[trackIndex];
            musicSource.Play();
        }
    }

    private void ApplyMixerVolumes(GameRuntimeSettings settings)
    {
        if (mainAudioMixer == null)
        {
            Debug.LogWarning("AudiomixerManager: No AudioMixer assigned.");
            return;
        }

        SetMixerVolume("MasterVolume", settings.masterVolume);
        SetMixerVolume("MusicVolume", settings.musicVolume);
        SetMixerVolume("SFXVolume", settings.sfxVolume);
        SetMixerVolume("SpeechVolume", settings.speechVolume);
    }

    private void SetMixerVolume(string parameterName, float normalizedValue)
    {
        float clampedValue = Mathf.Clamp(normalizedValue, 0.0001f, 1f);
        float volumeInDb = Mathf.Log10(clampedValue) * 20f;

        bool success = mainAudioMixer.SetFloat(parameterName, volumeInDb);

        if (!success)
        {
            Debug.LogWarning($"AudiomixerManager: Mixer parameter '{parameterName}' not found or not exposed.");
        }
    }
}
