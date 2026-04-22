using UnityEngine;

public class BarrelController : MonoBehaviour
{
    [Header("References")]
    // Light used to simulate the flickering fire glow.
    public Light fireLight;

    // Transform of the flame object that should face the camera.
    public Transform flameTransform;

    // Audio source used for the looping fire sound.
    public AudioSource fireAudioSource;

    // Fire sound clip played at startup.
    public AudioClip fireAudioClip;

    [Header("Light Flicker")]
    // Minimum light intensity used for random flickering.
    public float minIntensity = 2f;

    // Maximum light intensity used for random flickering.
    public float maxIntensity = 5f;

    private void Start()
    {
        // Assign and play the fire sound if both source and clip are available.
        if (fireAudioSource != null && fireAudioClip != null)
        {
            fireAudioSource.clip = fireAudioClip;
            fireAudioSource.Play();
        }
    }

    private void Update()
    {
        // Randomly vary the light intensity each frame to create a flicker effect.
        if (fireLight != null)
        {
            fireLight.intensity = Random.Range(minIntensity, maxIntensity);
        }
    }

    private void LateUpdate()
    {
        // Stop if no flame transform is assigned.
        if (flameTransform == null)
            return;

        // Stop if no main camera is available.
        if (Camera.main == null)
            return;

        // Use the camera forward direction so the flame always faces the player.
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0f;

        // Keep the billboard rotation on the horizontal plane only.
        if (forward.sqrMagnitude > 0.001f)
            flameTransform.forward = forward;
    }
}