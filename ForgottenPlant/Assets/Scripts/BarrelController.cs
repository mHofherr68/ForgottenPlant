using UnityEngine;

public class BarrelController : MonoBehaviour
{
    [Header("References")]
    public Light fireLight;
    public Transform flameTransform;
    public AudioSource fireAudioSource;
    public AudioClip fireAudioClip;

    [Header("Light Flicker")]
    public float minIntensity = 2f;
    public float maxIntensity = 5f;

    [Header("Audio")]
    public bool playOnAwake = true;
    public bool loop = true;
    [Range(0f, 1f)] public float volume = 0.25f;
    public float pitch = 1f;
    public float minDistance = 1.5f;
    public float maxDistance = 8f;

    private void Start()
    {
        // Audio Setup
        if (fireAudioSource != null)
        {
            fireAudioSource.clip = fireAudioClip;
            fireAudioSource.loop = loop;
            fireAudioSource.playOnAwake = playOnAwake;
            fireAudioSource.spatialBlend = 1f;
            fireAudioSource.volume = volume;
            fireAudioSource.pitch = pitch;
            fireAudioSource.minDistance = minDistance;
            fireAudioSource.maxDistance = maxDistance;

            if (playOnAwake && fireAudioClip != null)
                fireAudioSource.Play();
        }
    }

    private void Update()
    {
        // === DEIN ORIGINAL FLICKER ===
        if (fireLight != null)
        {
            fireLight.intensity = Random.Range(minIntensity, maxIntensity);
        }
    }

    private void LateUpdate()
    {
        // === DEIN ORIGINAL BILLBOARD ===
        if (flameTransform == null) return;
        if (Camera.main == null) return;

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            flameTransform.forward = forward;
    }
}
