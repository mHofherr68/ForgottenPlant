using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EnemyVoice : MonoBehaviour
{
    [Header("Voice Clips (Index-based)")]
    [SerializeField] private AudioClip[] voiceClips;

    [Header("Settings")]
    [SerializeField] private bool randomizePitch = true;
    [SerializeField] private Vector2 pitchRange = new Vector2(0.95f, 1.05f);

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayVoice(int index)
    {
        if (voiceClips == null || voiceClips.Length == 0)
        {
            if (debugLog)
                Debug.LogWarning($"{name}: No voice clips assigned.");
            return;
        }

        if (index < 0 || index >= voiceClips.Length)
        {
            if (debugLog)
                Debug.LogWarning($"{name}: Voice index {index} out of range.");
            return;
        }

        AudioClip clip = voiceClips[index];

        if (clip == null)
        {
            if (debugLog)
                Debug.LogWarning($"{name}: Voice clip at index {index} is null.");
            return;
        }

        // Monophones Verhalten:
        // neuer Voice-Clip schneidet den alten sofort ab
        if (audioSource.isPlaying)
            audioSource.Stop();

        if (randomizePitch)
            audioSource.pitch = Random.Range(pitchRange.x, pitchRange.y);
        else
            audioSource.pitch = 1f;

        audioSource.clip = clip;
        audioSource.Play();

        if (debugLog)
            Debug.Log($"{name}: Playing voice index {index}");
    }

    public void StopVoice()
    {
        if (audioSource == null)
            return;

        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    public bool IsPlaying => audioSource != null && audioSource.isPlaying;
}