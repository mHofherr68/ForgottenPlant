//using UnityEngine;
//using UnityEngine.InputSystem;

//public class WeaponSway : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private WeaponObstacleCheck obstacleCheck;

//    [Header("Sway Settings")]
//    [SerializeField] private float swayAmount = 0.05f;
//    [SerializeField] private float maxSwayAmount = 0.08f;
//    [SerializeField] private float smoothSpeed = 8f;

//    [Header("Rotation Settings")]
//    [SerializeField] private float rotationAmount = 4f;
//    [SerializeField] private float maxRotationAmount = 6f;

//    [Header("Debug")]
//    [SerializeField] private bool debugValues = false;

//    private Vector3 initialLocalPosition;
//    private Quaternion initialLocalRotation;

//    private void Awake()
//    {
//        initialLocalPosition = transform.localPosition;
//        initialLocalRotation = transform.localRotation;
//    }

//    private void LateUpdate()
//    {
//        if (Mouse.current == null)
//            return;

//        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

//        float moveX = Mathf.Clamp(-mouseDelta.x * swayAmount * Time.deltaTime, -maxSwayAmount, maxSwayAmount);
//        float moveY = Mathf.Clamp(-mouseDelta.y * swayAmount * Time.deltaTime, -maxSwayAmount, maxSwayAmount);

//        float obstacleOffsetX = 0f;
//        if (obstacleCheck != null)
//            obstacleOffsetX = obstacleCheck.CurrentOffsetX;

//        Vector3 targetPosition = initialLocalPosition + new Vector3(moveX + obstacleOffsetX, moveY, 0f);

//        float rotX = Mathf.Clamp(mouseDelta.y * rotationAmount * Time.deltaTime, -maxRotationAmount, maxRotationAmount);
//        float rotY = Mathf.Clamp(-mouseDelta.x * rotationAmount * Time.deltaTime, -maxRotationAmount, maxRotationAmount);

//        Quaternion targetRotation = initialLocalRotation * Quaternion.Euler(rotX, rotY, 0f);

//        transform.localPosition = Vector3.Lerp(
//            transform.localPosition,
//            targetPosition,
//            smoothSpeed * Time.deltaTime
//        );

//        transform.localRotation = Quaternion.Slerp(
//            transform.localRotation,
//            targetRotation,
//            smoothSpeed * Time.deltaTime
//        );

//        if (debugValues)
//        {
//            Debug.Log($"WeaponSway -> MouseDelta: {mouseDelta}, ObstacleOffsetX: {obstacleOffsetX}");
//        }
//    }
//}
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponSway : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private WeaponObstacleCheck obstacleCheck;

    [Header("Sway Settings")]
    [SerializeField] private float swayAmount = 0.05f;
    [SerializeField] private float maxSwayAmount = 0.08f;
    [SerializeField] private float smoothSpeed = 8f;

    [Header("Rotation Settings")]
    [SerializeField] private float rotationAmount = 4f;
    [SerializeField] private float maxRotationAmount = 6f;

    [Header("Debug")]
    [SerializeField] private bool debugValues = false;

    private Vector3 initialLocalPosition;
    private Quaternion initialLocalRotation;

    private void Awake()
    {
        initialLocalPosition = transform.localPosition;
        initialLocalRotation = transform.localRotation;
    }

    private void LateUpdate()
    {
        if (Mouse.current == null)
            return;

        Vector2 mouseDelta = Mouse.current.delta.ReadValue();

        float moveX = Mathf.Clamp(-mouseDelta.x * swayAmount * Time.deltaTime, -maxSwayAmount, maxSwayAmount);
        float moveY = Mathf.Clamp(-mouseDelta.y * swayAmount * Time.deltaTime, -maxSwayAmount, maxSwayAmount);

        float obstacleOffsetX = 0f;
        float obstacleOffsetZ = 0f;

        if (obstacleCheck != null)
        {
            obstacleOffsetX = obstacleCheck.CurrentOffsetX;
            obstacleOffsetZ = obstacleCheck.CurrentOffsetZ;
        }

        Vector3 targetPosition = initialLocalPosition + new Vector3(
            moveX + obstacleOffsetX,
            moveY,
            obstacleOffsetZ
        );

        float rotX = Mathf.Clamp(mouseDelta.y * rotationAmount * Time.deltaTime, -maxRotationAmount, maxRotationAmount);
        float rotY = Mathf.Clamp(-mouseDelta.x * rotationAmount * Time.deltaTime, -maxRotationAmount, maxRotationAmount);

        Quaternion targetRotation = initialLocalRotation * Quaternion.Euler(rotX, rotY, 0f);

        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            smoothSpeed * Time.deltaTime
        );

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