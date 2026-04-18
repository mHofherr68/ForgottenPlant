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

    private void Start()
    {
        if (fireAudioSource != null && fireAudioClip != null)
        {
            fireAudioSource.clip = fireAudioClip;
            fireAudioSource.Play();
        }
    }

    private void Update()
    {
        if (fireLight != null)
        {
            fireLight.intensity = Random.Range(minIntensity, maxIntensity);
        }
    }

    private void LateUpdate()
    {
        if (flameTransform == null)
            return;

        if (Camera.main == null)
            return;

        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude > 0.001f)
            flameTransform.forward = forward;
    }
}