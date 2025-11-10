using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementSystem : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float crouchSpeed = 2.5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform playerCamera;
    private float xRotation = 0f;

    [Header("Crouch Settings")]
    public float crouchHeight = .5f;
    public float standingHeight = 1f;

    public CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool isCrouching = false;
    public bool isBlocked = false;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if (isBlocked)
        {
            return;
        }
        MouseLook();
        MovePlayer();
        HandleCrouch();
    }

    void MovePlayer()
    {
        // Ground check
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
            velocity.y = -2f;

        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float currentSpeed = isCrouching && isGrounded ? crouchSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = true;
            controller.height = crouchHeight;
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            isCrouching = false;
            controller.height = standingHeight;
        }
    }
    public void Block()
    {
        isBlocked = !isBlocked;

        if (isBlocked)
        {
            velocity = Vector3.zero; // Reset all movement, especially vertical
            controller.Move(new Vector3());
        }
    }
}
