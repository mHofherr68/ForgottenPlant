/*using System.Collections;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private PlayerController playerController;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float senseX = 150f;
    [SerializeField] private float senseY = 150f;

    [Header("Vertical Look Limits")]
    [SerializeField] private float topLookAngle = -62f;
    [SerializeField] private float bottomLookAngle = 45f;

    [Header("Lean Settings")]
    [SerializeField] private float leanAmount = 0.3f;
    [SerializeField] private float leanAngle = 12f;
    [SerializeField] private float leanSpeed = 8f;

    [Header("Lean Collision")]
    [SerializeField] private float leanCheckDistance = 0.5f;
    [SerializeField] private LayerMask leanObstacleMask;

    [Header("Debug")]
    [SerializeField] private bool showLeanDebug = true;

    private InputSystem_Actions controls;
    private Vector2 lookInput;
    private float xRotation;

    private float baseSenseX;
    private float baseSenseY;

    private float currentSenseX;
    private float currentSenseY;
    private bool invertY;

    private float currentLean;
    private float targetLean;

    private Vector3 initialLocalPosition;

    public float CurrentXRotation => xRotation;

    private void Awake()
    {
        baseSenseX = senseX;
        baseSenseY = senseY;

        currentSenseX = baseSenseX;
        currentSenseY = baseSenseY;

        controls = new InputSystem_Actions();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        LockCursor();
        initialLocalPosition = transform.localPosition;
        StartCoroutine(ApplyMouseSettingsNextFrame());
    }

    private IEnumerator ApplyMouseSettingsNextFrame()
    {
        yield return null;

        if (GameSettingsManager.Instance == null)
            yield break;

        ApplyMouseSettings(
            GameSettingsManager.Instance.LiveSettings.mouseSensitivity,
            GameSettingsManager.Instance.LiveSettings.invertY
        );
    }

    public void ApplyMouseSettings(float sensitivityOffset, bool invertYSetting)
    {
        float multiplier = 1f + sensitivityOffset;

        currentSenseX = baseSenseX * multiplier;
        currentSenseY = baseSenseY * multiplier;

        invertY = invertYSetting;
    }

    private void Update()
    {
        HandleLeanInput();
        HandleLookInput();
        ApplyLean();
    }

    private void HandleLookInput()
    {
        float mouseX = lookInput.x * currentSenseX * Time.deltaTime;
        float mouseY = lookInput.y * currentSenseY * Time.deltaTime;

        if (invertY)
            mouseY *= -1f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topLookAngle, bottomLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentLean * leanAngle);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleLeanInput()
    {
        bool canLean = CanLean();

        if (!canLean)
        {
            targetLean = 0f;
            return;
        }

        if (controls.Player.LeanLeft.IsPressed())
        {
            targetLean = -1f;
        }
        else if (controls.Player.LeanRight.IsPressed())
        {
            targetLean = 1f;
        }
        else
        {
            targetLean = 0f;
        }
    }

    private void ApplyLean()
    {
        float allowedLean = GetAllowedLeanAmount(targetLean);

        currentLean = Mathf.Lerp(currentLean, allowedLean, leanSpeed * Time.deltaTime);

        Vector3 leanOffset = new Vector3(currentLean * leanAmount, 0f, 0f);
        transform.localPosition = initialLocalPosition + leanOffset;
    }

    private float GetAllowedLeanAmount(float desiredLean)
    {
        if (desiredLean == 0f)
            return 0f;

        Vector3 direction = desiredLean > 0f ? transform.right : -transform.right;

        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, leanCheckDistance, leanObstacleMask))
        {
            float distanceFactor = hit.distance / leanCheckDistance;
            return desiredLean * distanceFactor;
        }

        return desiredLean;
    }

    private bool CanLean()
    {
        if (playerController == null)
            return false;

        if (!playerController.IsGrounded)
            return false;

        bool isMovingForwardOrBackward = Mathf.Abs(playerController.MoveInput.y) > 0.01f;

        if (isMovingForwardOrBackward)
            return false;

        return true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 🔥 GIZMOS
    private void OnDrawGizmosSelected()
    {
        if (!showLeanDebug)
            return;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(transform.position, transform.position + transform.right * leanCheckDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position - transform.right * leanCheckDistance);
    }
}*/
/*using System.Collections;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private PlayerController playerController;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float senseX = 150f;
    [SerializeField] private float senseY = 150f;

    [Header("Vertical Look Limits")]
    [SerializeField] private float topLookAngle = -62f;
    [SerializeField] private float bottomLookAngle = 45f;

    [Header("Lean Settings")]
    [SerializeField] private float leanAmount = 0.3f;
    [SerializeField] private float leanAngle = 12f;
    [SerializeField] private float leanSpeed = 8f;

    [Header("Lean Collision")]
    [SerializeField] private float leanCheckDistance = 0.5f;
    [SerializeField] private float leanSphereRadius = 0.2f;
    [SerializeField] private LayerMask leanObstacleMask;

    [Header("Debug")]
    [SerializeField] private bool showLeanDebug = true;

    private InputSystem_Actions controls;
    private Vector2 lookInput;
    private float xRotation;

    private float baseSenseX;
    private float baseSenseY;

    private float currentSenseX;
    private float currentSenseY;
    private bool invertY;

    private float currentLean;
    private float targetLean;

    private Vector3 initialLocalPosition;

    public float CurrentXRotation => xRotation;

    private void Awake()
    {
        baseSenseX = senseX;
        baseSenseY = senseY;

        currentSenseX = baseSenseX;
        currentSenseY = baseSenseY;

        controls = new InputSystem_Actions();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        LockCursor();
        initialLocalPosition = transform.localPosition;
        StartCoroutine(ApplyMouseSettingsNextFrame());
    }

    private IEnumerator ApplyMouseSettingsNextFrame()
    {
        yield return null;

        if (GameSettingsManager.Instance == null)
            yield break;

        ApplyMouseSettings(
            GameSettingsManager.Instance.LiveSettings.mouseSensitivity,
            GameSettingsManager.Instance.LiveSettings.invertY
        );
    }

    public void ApplyMouseSettings(float sensitivityOffset, bool invertYSetting)
    {
        float multiplier = 1f + sensitivityOffset;

        currentSenseX = baseSenseX * multiplier;
        currentSenseY = baseSenseY * multiplier;

        invertY = invertYSetting;
    }

    private void Update()
    {
        HandleLeanInput();
        HandleLookInput();
        ApplyLean();
    }

    private void HandleLookInput()
    {
        float mouseX = lookInput.x * currentSenseX * Time.deltaTime;
        float mouseY = lookInput.y * currentSenseY * Time.deltaTime;

        if (invertY)
            mouseY *= -1f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topLookAngle, bottomLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentLean * leanAngle);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleLeanInput()
    {
        bool canLean = CanLean();

        if (!canLean)
        {
            targetLean = 0f;
            return;
        }

        if (controls.Player.LeanLeft.IsPressed())
        {
            targetLean = -1f;
        }
        else if (controls.Player.LeanRight.IsPressed())
        {
            targetLean = 1f;
        }
        else
        {
            targetLean = 0f;
        }
    }

    private void ApplyLean()
    {
        float allowedLean = GetAllowedLeanAmount(targetLean);

        currentLean = Mathf.Lerp(currentLean, allowedLean, leanSpeed * Time.deltaTime);

        Vector3 leanOffset = new Vector3(currentLean * leanAmount, 0f, 0f);
        transform.localPosition = initialLocalPosition + leanOffset;
    }

    private float GetAllowedLeanAmount(float desiredLean)
    {
        if (desiredLean == 0f)
            return 0f;

        // 👉 Zielposition berechnen
        Vector3 desiredOffset = new Vector3(desiredLean * leanAmount, 0f, 0f);
        Vector3 targetPosition = initialLocalPosition + desiredOffset;

        Vector3 worldCurrent = transform.parent.TransformPoint(initialLocalPosition);
        Vector3 worldTarget = transform.parent.TransformPoint(targetPosition);

        Vector3 direction = (worldTarget - worldCurrent).normalized;
        float distance = Vector3.Distance(worldCurrent, worldTarget);

        if (Physics.SphereCast(worldCurrent, leanSphereRadius, direction, out RaycastHit hit, distance, leanObstacleMask))
        {
            float safeDistance = hit.distance / distance;
            return desiredLean * safeDistance;
        }

        return desiredLean;
    }

    private bool CanLean()
    {
        if (playerController == null)
            return false;

        if (!playerController.IsGrounded)
            return false;

        bool isMovingForwardOrBackward = Mathf.Abs(playerController.MoveInput.y) > 0.01f;

        if (isMovingForwardOrBackward)
            return false;

        return true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // 🔥 Debug Gizmos
    /*private void OnDrawGizmosSelected()
    {
        if (!showLeanDebug)
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position + transform.right * leanCheckDistance, leanSphereRadius);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position - transform.right * leanCheckDistance, leanSphereRadius);
    }*/
/* private void OnDrawGizmosSelected()
 {
     if (!showLeanDebug || transform.parent == null)
         return;

     // 👉 FIX: benutze aktuelle Position im Editor
     Vector3 baseLocalPos = Application.isPlaying
         ? initialLocalPosition
         : transform.localPosition;

     // Links
     Vector3 leftLocal = baseLocalPos + new Vector3(-leanAmount, 0f, 0f);
     Vector3 leftWorld = transform.parent.TransformPoint(leftLocal);

     Gizmos.color = Color.magenta;
     Gizmos.DrawWireSphere(leftWorld, leanSphereRadius);

     // Rechts
     Vector3 rightLocal = baseLocalPos + new Vector3(leanAmount, 0f, 0f);
     Vector3 rightWorld = transform.parent.TransformPoint(rightLocal);

     Gizmos.color = Color.cyan;
     Gizmos.DrawWireSphere(rightWorld, leanSphereRadius);
 }
}*/
/*using System.Collections;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private PlayerController playerController;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float senseX = 150f;
    [SerializeField] private float senseY = 150f;

    [Header("Vertical Look Limits")]
    [SerializeField] private float topLookAngle = -62f;
    [SerializeField] private float bottomLookAngle = 45f;

    [Header("Lean Settings")]
    [SerializeField] private float leanAmount = 0.3f;
    [SerializeField] private float leanAngle = 12f;
    [SerializeField] private float leanSpeed = 8f;

    [Header("Lean Collision")]
    [SerializeField] private float leanSphereRadius = 0.2f;
    [SerializeField] private LayerMask leanObstacleMask;

    [Header("Crouch Settings")]                             // New
    [SerializeField] private float standingY = 1.6f;
    [SerializeField] private float crouchY = 0.9f;
    [SerializeField] private float crouchSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showLeanDebug = true;

    private InputSystem_Actions controls;
    private Vector2 lookInput;
    private float xRotation;

    private float baseSenseX;
    private float baseSenseY;

    private float currentSenseX;
    private float currentSenseY;
    private bool invertY;

    private float currentLean;
    private float targetLean;

    private float currentY;                                 // New

    private Vector3 initialLocalPosition;

    public float CurrentXRotation => xRotation;

    private void Awake()
    {
        baseSenseX = senseX;
        baseSenseY = senseY;

        currentSenseX = baseSenseX;
        currentSenseY = baseSenseY;

        controls = new InputSystem_Actions();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Start()
    {
        currentY = initialLocalPosition.y;                  // new
        LockCursor();
        initialLocalPosition = transform.localPosition;
        StartCoroutine(ApplyMouseSettingsNextFrame());
    }

    private IEnumerator ApplyMouseSettingsNextFrame()
    {
        yield return null;

        if (GameSettingsManager.Instance == null)
            yield break;

        ApplyMouseSettings(
            GameSettingsManager.Instance.LiveSettings.mouseSensitivity,
            GameSettingsManager.Instance.LiveSettings.invertY
        );
    }

    public void ApplyMouseSettings(float sensitivityOffset, bool invertYSetting)
    {
        float multiplier = 1f + sensitivityOffset;

        currentSenseX = baseSenseX * multiplier;
        currentSenseY = baseSenseY * multiplier;

        invertY = invertYSetting;
    }

    private void Update()
    {
        HandleLeanInput();
        HandleLookInput();
        ApplyLean();
    }

    private void HandleLookInput()
    {
        float mouseX = lookInput.x * currentSenseX * Time.deltaTime;
        float mouseY = lookInput.y * currentSenseY * Time.deltaTime;

        if (invertY)
            mouseY *= -1f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topLookAngle, bottomLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentLean * leanAngle);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleLeanInput()
    {
        bool canLean = CanLean();

        if (!canLean)
        {
            targetLean = 0f;
            return;
        }

        if (controls.Player.LeanLeft.IsPressed())
        {
            targetLean = -1f;
        }
        else if (controls.Player.LeanRight.IsPressed())
        {
            targetLean = 1f;
        }
        else
        {
            targetLean = 0f;
        }
    }

    /*private void ApplyLean()
    {
        float allowedLean = GetAllowedLeanAmount(targetLean);

        currentLean = Mathf.Lerp(currentLean, allowedLean, leanSpeed * Time.deltaTime);

        Vector3 leanOffset = new Vector3(currentLean * leanAmount, 0f, 0f);
        transform.localPosition = initialLocalPosition + leanOffset;
    }*/

/*private void ApplyLean()
{
    float allowedLean = GetAllowedLeanAmount(targetLean);

    currentLean = Mathf.Lerp(currentLean, allowedLean, leanSpeed * Time.deltaTime);

    // 🔥 Crouch Y berechnen
    float targetY = playerController != null && playerController.IsCrouching
        ? crouchY
        : standingY;

    currentY = Mathf.Lerp(currentY, targetY, crouchSpeed * Time.deltaTime);

    // 🔥 Kombination
    Vector3 leanOffset = new Vector3(currentLean * leanAmount, currentY, 0f);
    transform.localPosition = new Vector3(leanOffset.x, leanOffset.y, initialLocalPosition.z);
}

private float GetAllowedLeanAmount(float desiredLean)
{
    if (desiredLean == 0f || transform.parent == null)
        return 0f;

    GetLeanCastData(desiredLean, out Vector3 worldStart, out Vector3 worldTarget, out Vector3 direction, out float distance);

    if (distance <= 0.0001f)
        return desiredLean;

    if (Physics.SphereCast(worldStart, leanSphereRadius, direction, out RaycastHit hit, distance, leanObstacleMask, QueryTriggerInteraction.Ignore))
    {
        float safeFactor = hit.distance / distance;
        return desiredLean * safeFactor;
    }

    return desiredLean;
}

private void GetLeanCastData(float desiredLean, out Vector3 worldStart, out Vector3 worldTarget, out Vector3 direction, out float distance)
{
    Vector3 baseLocalPos = Application.isPlaying ? initialLocalPosition : transform.localPosition;

    Vector3 targetLocalPos = baseLocalPos + new Vector3(desiredLean * leanAmount, 0f, 0f);

    worldStart = transform.parent.TransformPoint(baseLocalPos);
    worldTarget = transform.parent.TransformPoint(targetLocalPos);

    Vector3 delta = worldTarget - worldStart;
    distance = delta.magnitude;
    direction = distance > 0.0001f ? delta / distance : Vector3.zero;
}

private bool CanLean()
{
    if (playerController == null)
        return false;

    if (!playerController.IsGrounded)
        return false;

    bool isMovingForwardOrBackward = Mathf.Abs(playerController.MoveInput.y) > 0.01f;

    if (isMovingForwardOrBackward)
        return false;

    return true;
}

private void LockCursor()
{
    Cursor.lockState = CursorLockMode.Locked;
    Cursor.visible = false;
}

private void OnDrawGizmosSelected()
{
    if (!showLeanDebug || transform.parent == null)
        return;

    Vector3 baseLocalPos = Application.isPlaying ? initialLocalPosition : transform.localPosition;
    Vector3 worldCenter = transform.parent.TransformPoint(baseLocalPos);

    // Mitte
    Gizmos.color = Color.white;
    Gizmos.DrawWireSphere(worldCenter, leanSphereRadius);

    // Links
    GetLeanCastData(-1f, out Vector3 leftStart, out Vector3 leftTarget, out _, out _);
    Gizmos.color = Color.magenta;
    Gizmos.DrawLine(leftStart, leftTarget);
    Gizmos.DrawWireSphere(leftTarget, leanSphereRadius);

    // Rechts
    GetLeanCastData(1f, out Vector3 rightStart, out Vector3 rightTarget, out _, out _);
    Gizmos.color = Color.cyan;
    Gizmos.DrawLine(rightStart, rightTarget);
    Gizmos.DrawWireSphere(rightTarget, leanSphereRadius);
}
}*/
using System.Collections;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;
    [SerializeField] private PlayerController playerController;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float senseX = 150f;
    [SerializeField] private float senseY = 150f;

    [Header("Vertical Look Limits")]
    [SerializeField] private float topLookAngle = -62f;
    [SerializeField] private float bottomLookAngle = 45f;

    [Header("Lean Settings")]
    [SerializeField] private float leanAmount = 0.3f;
    [SerializeField] private float leanAngle = 12f;
    [SerializeField] private float leanSpeed = 8f;

    [Header("Lean Collision")]
    [SerializeField] private float leanSphereRadius = 0.2f;
    [SerializeField] private LayerMask leanObstacleMask;

    [Header("Crouch Settings")]
    [SerializeField] private float standingY = 1.6f;
    [SerializeField] private float crouchY = 0.9f;
    [SerializeField] private float crouchSpeed = 10f;

    [Header("Debug")]
    [SerializeField] private bool showLeanDebug = true;

    private InputSystem_Actions controls;
    private Vector2 lookInput;
    private float xRotation;

    private float baseSenseX;
    private float baseSenseY;

    private float currentSenseX;
    private float currentSenseY;
    private bool invertY;

    private float currentLean;
    private float targetLean;

    private float currentY;

    private Vector3 initialLocalPosition;

    public float CurrentXRotation => xRotation;

    private void Awake()
    {
        baseSenseX = senseX;
        baseSenseY = senseY;

        currentSenseX = baseSenseX;
        currentSenseY = baseSenseY;

        controls = new InputSystem_Actions();

        controls.Player.Look.performed += ctx => lookInput = ctx.ReadValue<Vector2>();
        controls.Player.Look.canceled += ctx => lookInput = Vector2.zero;
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Start()
    {
        LockCursor();

        // ✅ RICHTIGE REIHENFOLGE
        initialLocalPosition = transform.localPosition;

        // ✅ direkt korrekt setzen (kein Lerp am Anfang!)
        currentY = standingY;

        Vector3 pos = transform.localPosition;
        pos.y = standingY;
        transform.localPosition = pos;

        StartCoroutine(ApplyMouseSettingsNextFrame());
    }

    private IEnumerator ApplyMouseSettingsNextFrame()
    {
        yield return null;

        if (GameSettingsManager.Instance == null)
            yield break;

        ApplyMouseSettings(
            GameSettingsManager.Instance.LiveSettings.mouseSensitivity,
            GameSettingsManager.Instance.LiveSettings.invertY
        );
    }

    public void ApplyMouseSettings(float sensitivityOffset, bool invertYSetting)
    {
        float multiplier = 1f + sensitivityOffset;

        currentSenseX = baseSenseX * multiplier;
        currentSenseY = baseSenseY * multiplier;

        invertY = invertYSetting;
    }

    private void Update()
    {
        HandleLeanInput();
        HandleLookInput();
        ApplyLean();
    }

    private void HandleLookInput()
    {
        float mouseX = lookInput.x * currentSenseX * Time.deltaTime;
        float mouseY = lookInput.y * currentSenseY * Time.deltaTime;

        if (invertY)
            mouseY *= -1f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topLookAngle, bottomLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, currentLean * leanAngle);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void HandleLeanInput()
    {
        if (!CanLean())
        {
            targetLean = 0f;
            return;
        }

        if (controls.Player.LeanLeft.IsPressed())
            targetLean = -1f;
        else if (controls.Player.LeanRight.IsPressed())
            targetLean = 1f;
        else
            targetLean = 0f;
    }

    private void ApplyLean()
    {
        float allowedLean = GetAllowedLeanAmount(targetLean);

        currentLean = Mathf.Lerp(currentLean, allowedLean, leanSpeed * Time.deltaTime);

        float targetY = playerController != null && playerController.IsCrouching
            ? crouchY
            : standingY;

        currentY = Mathf.Lerp(currentY, targetY, crouchSpeed * Time.deltaTime);

        // ✅ sauber kombiniert
        transform.localPosition = new Vector3(
            initialLocalPosition.x + currentLean * leanAmount,
            currentY,
            initialLocalPosition.z
        );
    }

    private float GetAllowedLeanAmount(float desiredLean)
    {
        if (desiredLean == 0f || transform.parent == null)
            return 0f;

        GetLeanCastData(desiredLean, out Vector3 start, out Vector3 target, out Vector3 dir, out float dist);

        if (dist <= 0.0001f)
            return desiredLean;

        if (Physics.SphereCast(start, leanSphereRadius, dir, out RaycastHit hit, dist, leanObstacleMask))
        {
            float safe = hit.distance / dist;
            return desiredLean * safe;
        }

        return desiredLean;
    }

    private void GetLeanCastData(float lean, out Vector3 start, out Vector3 target, out Vector3 dir, out float dist)
    {
        Vector3 baseLocal = Application.isPlaying ? initialLocalPosition : transform.localPosition;

        Vector3 targetLocal = baseLocal + new Vector3(lean * leanAmount, 0f, 0f);

        start = transform.parent.TransformPoint(baseLocal);
        target = transform.parent.TransformPoint(targetLocal);

        Vector3 delta = target - start;
        dist = delta.magnitude;
        dir = dist > 0.0001f ? delta / dist : Vector3.zero;
    }

    private bool CanLean()
    {
        if (playerController == null)
            return false;

        if (!playerController.IsGrounded)
            return false;

        if (Mathf.Abs(playerController.MoveInput.y) > 0.01f)
            return false;

        return true;
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!showLeanDebug || transform.parent == null)
            return;

        Vector3 baseLocal = Application.isPlaying ? initialLocalPosition : transform.localPosition;
        Vector3 center = transform.parent.TransformPoint(baseLocal);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(center, leanSphereRadius);

        GetLeanCastData(-1f, out Vector3 ls, out Vector3 lt, out _, out _);
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(ls, lt);
        Gizmos.DrawWireSphere(lt, leanSphereRadius);

        GetLeanCastData(1f, out Vector3 rs, out Vector3 rt, out _, out _);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(rs, rt);
        Gizmos.DrawWireSphere(rt, leanSphereRadius);
    }
}