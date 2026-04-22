using System.Collections;
using Unity.AI.Navigation;
using UnityEngine;

public class FenceGateController : MonoBehaviour
{
    // Defines the local axis around which the gate will rotate when falling.
    public enum FallAxis
    {
        X,
        Y,
        Z
    }

    [Header("Fall Settings")]
    // Axis used for the falling rotation.
    [SerializeField] private FallAxis fallAxis = FallAxis.Z;

    // Final rotation angle reached when the gate has fully fallen.
    [SerializeField] private float fallAngle = 90f;

    // Duration of the falling animation.
    [SerializeField] private float fallDuration = 1f;

    [Header("Direction")]
    // Inverts the fall direction along the selected axis.
    [SerializeField] private bool invertDirection = false;

    [Header("Position Y")]
    // Base local Y position of the gate before falling.
    [SerializeField] private float baseY = 0.5f;

    // Additional Y offset applied while the gate is falling.
    [SerializeField] private float fallYOffset = 0.18f;

    [Header("Audio")]
    // Audio source used to play the impact sound.
    [SerializeField] private AudioSource gateAudioSource;

    // Sound played when the gate finishes falling.
    [SerializeField] private AudioClip impactClip;

    [Header("Collider")]
    // Collider that blocks passage while the gate is still standing.
    [SerializeField] private Collider gateCollider;

    [Header("NavMesh Link")]
    // NavMesh link that becomes active after the gate has fallen.
    [SerializeField] private NavMeshLink navMeshLink;

    [Header("Layer Switch")]
    // Layer mask used while the gate is still standing.
    [SerializeField] private LayerMask standingLayer;

    // Layer mask used after the gate has fallen.
    [SerializeField] private LayerMask fallenLayer;

    [Header("Test")]
    // Inspector test trigger to force the gate to fall during runtime.
    [SerializeField] private bool testTrigger = false;

    [Header("Debug")]
    // Enables debug logging for gate state changes.
    [SerializeField] private bool debugLog = false;

    // Initial local rotation before the gate starts falling.
    private Quaternion startRotation;

    // Final target local rotation after the fall is complete.
    private Quaternion targetRotation;

    // Initial local position of the gate.
    private Vector3 startPosition;

    // True once the gate has been triggered.
    private bool isTriggered = false;

    // True while the falling animation is currently running.
    private bool isFalling = false;

    // Public read-only access to the trigger state.
    public bool IsTriggered => isTriggered;

    // Public read-only access to the falling state.
    public bool IsFalling => isFalling;

    private void Awake()
    {
        // Cache the starting local rotation and position.
        startRotation = transform.localRotation;
        startPosition = transform.localPosition;

        // Force the gate to use the configured base Y position.
        transform.localPosition = new Vector3(
            startPosition.x,
            baseY,
            startPosition.z
        );

        // Store the corrected start position and precalculate the final target rotation.
        startPosition = transform.localPosition;
        targetRotation = GetTargetRotation();

        // Auto-assign optional references if not set manually.
        if (gateCollider == null)
            gateCollider = GetComponent<Collider>();

        if (navMeshLink == null)
            navMeshLink = GetComponent<NavMeshLink>();

        // The NavMesh link should only become active after the gate has fallen.
        if (navMeshLink != null)
            navMeshLink.enabled = false;

        // Apply the standing layer to this object and all children.
        SetLayerRecursively(gameObject, GetFirstLayerFromMask(standingLayer));
    }

    private void Update()
    {
        // Inspector-only test trigger for manual runtime testing.
        if (testTrigger)
        {
            testTrigger = false;
            TriggerGate();
        }
    }

    private Quaternion GetTargetRotation()
    {
        // Determine the signed fall direction.
        float dir = invertDirection ? -1f : 1f;
        Vector3 rot = Vector3.zero;

        // Build the Euler rotation for the selected fall axis.
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

        // Return the final local rotation relative to the starting rotation.
        return startRotation * Quaternion.Euler(rot);
    }

    public void TriggerGate()
    {
        // Prevent the gate from being triggered more than once.
        if (isTriggered)
            return;

        isTriggered = true;
        StartCoroutine(FallRoutine());

        if (debugLog)
            Debug.Log($"{name}: Gate triggered.");
    }

    private IEnumerator FallRoutine()
    {
        // Mark the gate as currently falling.
        isFalling = true;

        float elapsed = 0f;
        Quaternion currentStartRot = transform.localRotation;

        while (elapsed < fallDuration)
        {
            elapsed += Time.deltaTime;

            // Normalize elapsed time and apply simple easing.
            float t = Mathf.Clamp01(elapsed / fallDuration);
            float easedT = t * t;

            // Interpolate rotation toward the target rotation.
            transform.localRotation = Quaternion.Lerp(currentStartRot, targetRotation, easedT);

            // Apply an additional vertical offset during the fall.
            float yOffset = fallYOffset * easedT;

            transform.localPosition = new Vector3(
                startPosition.x,
                baseY + yOffset,
                startPosition.z
            );

            yield return null;
        }

        // Snap to the final resting transform.
        transform.localRotation = targetRotation;
        transform.localPosition = new Vector3(
            startPosition.x,
            baseY + fallYOffset,
            startPosition.z
        );

        // Play the impact sound once the gate has fully fallen.
        PlayImpactSound();

        // Disable the blocking collider so the path becomes passable.
        if (gateCollider != null)
            gateCollider.enabled = false;

        // Enable the NavMesh link so AI can use the new path.
        if (navMeshLink != null)
            navMeshLink.enabled = true;

        // Switch the whole object hierarchy to the fallen layer.
        SetLayerRecursively(gameObject, GetFirstLayerFromMask(fallenLayer));

        isFalling = false;

        if (debugLog)
            Debug.Log($"{name}: Gate finished falling.");
    }

    private void PlayImpactSound()
    {
        // Play the impact sound only if both source and clip are available.
        if (gateAudioSource == null || impactClip == null)
            return;

        gateAudioSource.PlayOneShot(impactClip);
    }

    private void SetLayerRecursively(GameObject targetObject, int targetLayer)
    {
        // Ignore invalid layer values.
        if (targetLayer < 0)
            return;

        // Apply the target layer to this object.
        targetObject.layer = targetLayer;

        // Apply the same layer to all child objects.
        foreach (Transform child in targetObject.transform)
        {
            SetLayerRecursively(child.gameObject, targetLayer);
        }
    }

    private int GetFirstLayerFromMask(LayerMask mask)
    {
        // Extract the first valid layer index from the given layer mask.
        int maskValue = mask.value;

        for (int i = 0; i < 32; i++)
        {
            if ((maskValue & (1 << i)) != 0)
                return i;
        }

        return -1;
    }
}