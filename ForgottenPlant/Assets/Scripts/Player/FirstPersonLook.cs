using UnityEngine;          // Adapted from PIP2

public class FirstPersonLook : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerBody;

    [Header("Mouse Sensitivity")]
    [SerializeField] private float senseX = 150f;
    [SerializeField] private float senseY = 150f;

    [Header("Vertical Look Limits")]
    [SerializeField] private float minLookAngle = -62f;
    [SerializeField] private float maxLookAngle = 45f;

    private InputSystem_Actions controls;
    private Vector2 lookInput;
    private float xRotation;

    public float CurrentXRotation => xRotation;

    private void Awake()
    {
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
    }

    private void Update()
    {
        HandleLookInput();
    }

    private void HandleLookInput()
    {
        float mouseX = lookInput.x * senseX * Time.deltaTime;
        float mouseY = lookInput.y * senseY * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        playerBody.Rotate(Vector3.up * mouseX);
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}