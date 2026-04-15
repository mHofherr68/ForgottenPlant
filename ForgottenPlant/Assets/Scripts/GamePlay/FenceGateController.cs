using System.Collections;
using Unity.AI.Navigation;
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

    [Header("Collider")]
    [SerializeField] private Collider gateCollider;

    [Header("NavMesh Link")]
    [SerializeField] private NavMeshLink navMeshLink;

    [Header("Layer Switch")]
    [SerializeField] private LayerMask standingLayer;
    [SerializeField] private LayerMask fallenLayer;

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

        if (gateCollider == null)
            gateCollider = GetComponent<Collider>();

        if (navMeshLink == null)
            navMeshLink = GetComponent<NavMeshLink>();

        if (navMeshLink != null)
            navMeshLink.enabled = false;

        SetLayerRecursively(gameObject, GetFirstLayerFromMask(standingLayer));
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

        if (gateCollider != null)
            gateCollider.enabled = false;

        if (navMeshLink != null)
            navMeshLink.enabled = true;

        SetLayerRecursively(gameObject, GetFirstLayerFromMask(fallenLayer));

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

    private void SetLayerRecursively(GameObject targetObject, int targetLayer)
    {
        if (targetLayer < 0)
            return;

        targetObject.layer = targetLayer;

        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursively(child.gameObject, targetLayer);
        }
    }

    private int GetFirstLayerFromMask(LayerMask mask)
    {
        int maskValue = mask.value;

        for (int i = 0; i < 32; i++)
        {
            if ((maskValue & (1 << i)) != 0)
                return i;
        }

        return -1;
    }
}