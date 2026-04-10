using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform groundCheck;

    [Header("Camera")]
    [SerializeField] private Transform playerCamera;

    [Header("Spawn")]
    [SerializeField] private Transform playerSpawnPoint;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sneakSpeed = 2f;

    [Header("Crouch")]
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float standingHeight = 2f;
    [SerializeField] private float crouchCameraY = 0.9f;
    [SerializeField] private float standingCameraY = 1.6f;
    [SerializeField] private float crouchSpeed = 10f;

    [Header("Jump & Gravity")]
    [SerializeField] private float gravity = -20f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float fallMultiplier = 5f;

    [Header("Ground Check")]
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private InputSystem_Actions controls;

    private Vector2 moveInput;
    private Vector3 velocity;

    public bool IsGrounded { get; private set; }
    public bool IsSneaking { get; private set; }
    public bool IsCrouching { get; private set; }

    public Vector2 MoveInput => moveInput;

    private float CurrentSpeed => (IsSneaking || IsCrouching) ? sneakSpeed : walkSpeed;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => Jump();

        controls.Player.Sneak.performed += ctx => IsSneaking = true;
        controls.Player.Sneak.canceled += ctx => IsSneaking = false;

        controls.Player.Crouch.performed += ctx => ToggleCrouch();
    }

    private void Start()
    {
        ApplySpawnPoint();

        controller.height = standingHeight;                             //New
        controller.center = new Vector3(0f, standingHeight / 2f, 0f);
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        HandleCrouch();
        ApplyGravity();
    }

    private void ApplySpawnPoint()
    {
        if (playerSpawnPoint == null)
        {
            Debug.LogWarning("PlayerSpawnPoint not assigned!");
            return;
        }

        controller.enabled = false;

        transform.position = playerSpawnPoint.position;
        transform.rotation = Quaternion.Euler(0f, playerSpawnPoint.eulerAngles.y, 0f);

        controller.enabled = true;
    }

    private void HandleGroundCheck()
    {
        IsGrounded = Physics.CheckSphere(
            groundCheck.position,
            groundDistance,
            groundMask
        );

        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }

    private void HandleMovement()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * CurrentSpeed * Time.deltaTime);
    }

    private void HandleCrouch()
    {
        float targetHeight = IsCrouching ? crouchHeight : standingHeight;
        float currentHeight = controller.height;

        controller.height = Mathf.Lerp(currentHeight, targetHeight, crouchSpeed * Time.deltaTime);

        controller.center = new Vector3(0f, controller.height / 2f, 0f);

        float targetCamY = IsCrouching ? crouchCameraY : standingCameraY;
    }

    private void ToggleCrouch()
    {
        // Vorbereitung für später: Platz prüfen
        if (IsCrouching)
        {
            if (!CanStandUp())
                return;
        }

        IsCrouching = !IsCrouching;
    }

    private bool CanStandUp()
    {
        // aktuell simpel → später erweitern
        return true;
    }

    private void ApplyGravity()
    {
        if (velocity.y < 0f)
        {
            velocity.y += gravity * fallMultiplier * Time.deltaTime;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (!IsGrounded || IsCrouching)
            return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        IsGrounded = false;
    }
}