using System.Collections;
using UnityEngine;

public class FenceGateController : MonoBehaviour
{
    public enum FallAxis
    {
        X,
        Y,
        Z
    }

    [Header("Fall Settings")]
    [SerializeField] private FallAxis fallAxis = FallAxis.Z;
    [SerializeField] private float fallAngle = 90f;
    [SerializeField] private float fallDuration = 1f;

    [Header("Direction")]
    [SerializeField] private bool invertDirection = false;

    [Header("Position Y")]
    [SerializeField] private float baseY = 0.5f;
    [SerializeField] private float fallYOffset = 0.18f;

    [Header("Audio")]
    [SerializeField] private AudioSource gateAudioSource;
    [SerializeField] private AudioClip impactClip;

    [Header("Test")]
    [SerializeField] private bool testTrigger = false;

    [Header("Debug")]
    [SerializeField] private bool debugLog = false;

    private Quaternion startRotation;
    private Quaternion targetRotation;
    private Vector3 startPosition;

    private bool isTriggered = false;
    private bool isFalling = false;

    public bool IsTriggered => isTriggered;
    public bool IsFalling => isFalling;

    private void Awake()
    {
        startRotation = transform.localRotation;
        startPosition = transform.localPosition;

        transform.localPosition = new Vector3(
            startPosition.x,
            baseY,
            startPosition.z
        );

        startPosition = transform.localPosition;
        targetRotation = GetTargetRotation();
    }

    private void Update()
    {
        if (testTrigger)
        {
            testTrigger = false;
            TriggerGate();
        }
    }

    private Quaternion GetTargetRotation()
    {
        float dir = invertDirection ? -1f : 1f;
        Vector3 rot = Vector3.zero;

        switch (fallAxis)
        {
            case FallAxis.X:
                rot = new Vector3(fallAngle * dir, 0f, 0f);
                break;

            case FallAxis.Y:
                rot = new Vector3(0f, fallAngle * dir, 0f);
                break;

            case FallAxis.Z:
                rot = new Vector3(0f, 0f, fallAngle * dir);
                break;
        }

        return startRotation * Quaternion.Euler(rot);
    }

    public void TriggerGate()
    {
        if (isTriggered)
            return;

        isTriggered = true;
        StartCoroutine(FallRoutine());

        if (debugLog)
            Debug.Log($"{name}: Gate triggered.");
    }

    private IEnumerator FallRoutine()
    {
        isFalling = true;

        float elapsed = 0f;
        Quaternion currentStartRot = transform.localRotation;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;

            float t = Mathf.Clamp01(elapsed / fallDuration);

            // langsam starten, dann schneller werden
            float easedT = t * t;

            transform.localRotation = Quaternion.Lerp(currentStartRot, targetRotation, easedT);

            float yOffset = fallYOffset * easedT;

            transform.localPosition = new Vector3(
                startPosition.x,
                baseY + yOffset,
                startPosition.z
            );

            yield return null;
        }

        transform.localRotation = targetRotation;
        transform.localPosition = new Vector3(
            startPosition.x,
            baseY + fallYOffset,
            startPosition.z
        );

        PlayImpactSound();

        isFalling = false;

        if (debugLog)
            Debug.Log($"{name}: Gate finished falling.");
    }

    private void PlayImpactSound()
    {
        if (gateAudioSource == null || impactClip == null)
            return;

        gateAudioSource.PlayOneShot(impactClip);
    }

    public void ResetGate()
    {
        StopAllCoroutines();

        transform.localRotation = startRotation;
        transform.localPosition = new Vector3(
            startPosition.x,
            baseY,
            startPosition.z
        );

        targetRotation = GetTargetRotation();

        isTriggered = false;
        isFalling = false;

        if (debugLog)
            Debug.Log($"{name}: Gate reset.");
    }
}