/*using System.Collections;
using UnityEngine;

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float senseX = 150f;
    [SerializeField] private float senseY = 150f;

    [Header("Vertical Look Limits")]
    [SerializeField] private float topLookAngle = -62f;
    [SerializeField] private float bottomLookAngle = 45f;

    private InputSystem_Actions controls;
    private Vector2 lookInput;
    private float xRotation;

    private float baseSenseX;
    private float baseSenseY;

    private float currentSenseX;
    private float currentSenseY;
    private bool invertY;

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
        StartCoroutine(ApplyMouseSettingsNextFrame());
    }

    private IEnumerator ApplyMouseSettingsNextFrame()
    {
        yield return null;

        if (GameSettingsManager.Instance == null)
            yield break;

        ApplyMouseSettings(GameSettingsManager.Instance.LiveSettings.mouseSensitivity,
                           GameSettingsManager.Instance.LiveSettings.invertY);
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
        HandleLookInput();
    }

    private void HandleLookInput()
    {
        float mouseX = lookInput.x * currentSenseX * Time.deltaTime;
        float mouseY = lookInput.y * currentSenseY * Time.deltaTime;

        if (invertY)
            mouseY *= -1f;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, topLookAngle, bottomLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
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
        currentLean = Mathf.Lerp(currentLean, targetLean, leanSpeed * Time.deltaTime);

        Vector3 leanOffset = new Vector3(currentLean * leanAmount, 0f, 0f);
        transform.localPosition = initialLocalPosition + leanOffset;
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
}