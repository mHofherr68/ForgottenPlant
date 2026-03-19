using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private Transform groundCheck;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 6f;
    [SerializeField] private float sneakSpeed = 2f;

    [Header("Jump & Gravity")]
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float jumpHeight = 1.5f;

    [Header("Ground Check")]
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private InputSystem_Actions controls;

    private Vector2 moveInput;
    private Vector3 velocity;

    public bool IsGrounded { get; private set; }
    public bool IsSneaking { get; private set; }

    private float CurrentSpeed => IsSneaking ? sneakSpeed : walkSpeed;

    private void Awake()
    {
        controls = new InputSystem_Actions();

        controls.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        controls.Player.Jump.performed += ctx => Jump();

        controls.Player.Sneak.performed += ctx => IsSneaking = true;
        controls.Player.Sneak.canceled += ctx => IsSneaking = false;
    }

    private void OnEnable()
    {
        controls.Enable();
    }

    private void OnDisable()
    {
        controls.Disable();
    }

    private void Update()
    {
        HandleGroundCheck();
        HandleMovement();
        ApplyGravity();
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

    private void ApplyGravity()
    {
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Jump()
    {
        if (!IsGrounded) return;

        velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }
}
