using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    [Header("References")]
    // Optional obstacle check used to push the weapon back when near walls.
    [SerializeField] private WeaponObstacleCheck obstacleCheck;

    [Header("Sway Settings")]
    // Base amount of positional sway caused by mouse movement.
    [SerializeField] private float swayAmount = 0.05f;

    // Maximum positional sway offset.
    [SerializeField] private float maxSwayAmount = 0.08f;

    // Interpolation speed used for smoothing sway motion.
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Rotation Settings")]
    // Base amount of rotational sway caused by mouse movement.
    [SerializeField] private float rotationAmount = 4f;

    // Maximum rotational sway angle.
    [SerializeField] private float maxRotationAmount = 6f;

    [Header("Debug")]
    // Enables debug logs for mouse delta and obstacle offsets.
    [SerializeField] private bool debugValues = false;

    // Cached starting local position of the weapon.
    private Vector3 initialLocalPosition;

    // Cached starting local rotation of the weapon.
    private Quaternion initialLocalRotation;

    private void Awake()
    {
        // Store the starting local transform as the base pose for sway calculations.
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        // Stop if no mouse device is currently available.
        if (Mouse.current == null)
            return;

        // Read raw mouse delta for this frame.
        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        // Convert mouse movement into positional sway offsets.
        float moveX = Mathf.Clamp(-mouseDelta.x * swayAmount * Time.deltaTime, -maxSwayAmount, maxSwayAmount);
        float moveY = Mathf.Clamp(-mouseDelta.y * swayAmount * Time.deltaTime, -maxSwayAmount, maxSwayAmount);

        // Default obstacle offsets.
        float obstacleOffsetX = 0f;
        float obstacleOffsetZ = 0f;

        // Read obstacle-based offsets if an obstacle check component is assigned.
        if (obstacleCheck != null)
        {
            obstacleOffsetX = obstacleCheck.CurrentOffsetX;
            obstacleOffsetZ = obstacleCheck.CurrentOffsetZ;
        }

        // Build the final target local position using sway and obstacle offsets.
        Vector3 targetPosition = initialLocalPosition + new Vector3(
            moveX + obstacleOffsetX,
            moveY,
            obstacleOffsetZ
        );

        // Convert mouse movement into rotational sway offsets.
        float rotX = Mathf.Clamp(mouseDelta.y * rotationAmount * Time.deltaTime, -maxRotationAmount, maxRotationAmount);
        float rotY = Mathf.Clamp(-mouseDelta.x * rotationAmount * Time.deltaTime, -maxRotationAmount, maxRotationAmount);

        // Build the final target local rotation.
        Quaternion targetRotation = initialLocalRotation * Quaternion.Euler(rotX, rotY, 0f);

        // Smoothly move the weapon toward the target sway position.
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

        // Smoothly rotate the weapon toward the target sway rotation.
        transform.localRotation = Quaternion.Slerp(
            transform.localRotation,
            targetRotation,
            smoothSpeed * Time.deltaTime
        );

        if (debugValues)
        {
            Debug.Log($"WeaponSway -> MouseDelta: {mouseDelta}, OffsetX: {obstacleOffsetX}, OffsetZ: {obstacleOffsetZ}");
        }
    }
}