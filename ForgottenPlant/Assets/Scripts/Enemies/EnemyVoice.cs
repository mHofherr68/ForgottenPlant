using System.Collections;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyVoice : MonoBehaviour
{
    [System.Serializable]
    public class VoiceEntry
    {
        // Audio clip that should be played for this voice entry.
        public AudioClip clip;

        // Optional delay before the clip starts playing.
        public float startDelay = 0f;
    }

    [Header("Voice Clips (Index-based)")]
    // List of voice entries accessed by index.
    [SerializeField] private VoiceEntry[] voiceClips;

    [Header("Settings")]
    // Enables slight random pitch variation for more natural voice playback.
    [SerializeField] private bool randomizePitch = true;

    // Minimum and maximum pitch used when random pitch is enabled.
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Debug")]
    // Enables debug logs for voice playback and validation warnings.
    [SerializeField] private bool debugLog = false;

    // Cached AudioSource used for all voice playback.
    private AudioSource audioSource;

    // Stores the currently running delayed playback coroutine.
    private Coroutine playVoiceRoutine;

    private void Awake()
    {
        // Cache the AudioSource required by this component.
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayVoice(int index)
    {
        // Stop if no voice entries are assigned.
        if (voiceClips == null || voiceClips.Length == 0)
        {
            if (debugLog)
                Debug.LogWarning($"{name}: No voice clips assigned.");
            return;
        }

        // Stop if the requested index is outside the valid range.
        if (index < 0 || index >= voiceClips.Length)
        {
            if (debugLog)
                Debug.LogWarning($"{name}: Voice index {index} out of range.");
            return;
        }

        VoiceEntry entry = voiceClips[index];

        // Stop if the selected entry or its clip is missing.
        if (entry == null || entry.clip == null)
        {
            if (debugLog)
                Debug.LogWarning($"{name}: Voice clip at index {index} is null.");
            return;
        }

        // Monophonic behavior:
        // a new voice clip immediately interrupts the currently playing one.
        if (playVoiceRoutine != null)
        {
            StopCoroutine(playVoiceRoutine);
            playVoiceRoutine = null;
        }

        if (audioSource.isPlaying)
            audioSource.Stop();

        // Start playback, including optional start delay.
        playVoiceRoutine = StartCoroutine(PlayVoiceRoutine(index, entry));
    }

    private IEnumerator PlayVoiceRoutine(int index, VoiceEntry entry)
    {
        // Wait for the configured start delay before playback.
        if (entry.startDelay > 0f)
            yield return new WaitForSeconds(entry.startDelay);

        // Apply random pitch variation if enabled.
        if (randomizePitch)
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        else
            audioSource.pitch = 1f;

        // Assign the selected clip and start playback.
        audioSource.clip = entry.clip;
        audioSource.Play();

        if (debugLog)
            Debug.Log($"{name}: Playing voice index {index}");

        // Clear the routine reference once playback has started.
        playVoiceRoutine = null;
    }

    public void StopVoice()
    {
        // Stop if the AudioSource is not available.
        if (audioSource == null)
            return;

        // Cancel delayed playback if one is waiting to start.
        if (playVoiceRoutine != null)
        {
            StopCoroutine(playVoiceRoutine);
            playVoiceRoutine = null;
        }

        // Stop currently playing voice audio.
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    // Returns true while a voice clip is currently playing.
    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
}