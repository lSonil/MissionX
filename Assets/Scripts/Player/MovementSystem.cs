using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MovementSystem : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 5f;
    public float jumpHeight = 2f;
    public float gravity = -9.81f;

    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 100f;
    public Transform body;
    private float xRotation = 0f;

    [Header("Sprinting Settings")]
    public float sprintSpeed = 8f; // Changed from crouchSpeed
    public float sprintCounter = 10f; // Your counter of 10
    public float maxSprint = 10f;
    public float depleteRate = 0.5f; // Deplete by 0.5
    public float regenRate = 0.3f;   // Optional: regen when not sprinting
    private bool isSprinting = false;

    public CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool inAir = false;
    private bool isCrouching = false;
    public bool isBlocked = false;
    private Coroutine footstepRoutine;
    public void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    public void NetworkUpdate()
    {
        if (isBlocked) return;

        MouseLook();
        MovePlayer();
        HandleSprint();

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            StartCoroutine(MoveToCenter());
        }
    }
    IEnumerator MoveToCenter()
    {
        Block();
        transform.position = new Vector3(0,1,0);
        yield return new WaitForSeconds(.2f);
        Block();
    }
    void MovePlayer()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            if (inAir)
            {
                GetComponent<AudioSystem>().PlayLandEffect();
                inAir = false;
            }
            velocity.y = -2f;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        bool isMoving = (moveX != 0 || moveZ != 0);

        if (isMoving && footstepRoutine == null && isGrounded)
        {
            footstepRoutine = StartCoroutine(FootstepLoop());
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        float currentSpeed = (isSprinting && sprintCounter > 0) ? sprintSpeed : walkSpeed;

        controller.Move(move * currentSpeed * Time.deltaTime);

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            inAir = true;
            GetComponent<AudioSystem>().PlayJumpEffect();
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }
    IEnumerator FootstepLoop()
    {
        var audio = GetComponent<AudioSystem>();

        while (true)
        {
            float moveX = Input.GetAxis("Horizontal");
            float moveZ = Input.GetAxis("Vertical");

            if ((moveX == 0f && moveZ == 0f) || !isGrounded)
            {
                footstepRoutine = null;
                yield break;
            }

            audio.PlayFootstep();

            float stepDelay = isSprinting ? 0.3f : 0.5f;

            yield return new WaitForSeconds(stepDelay);
        }
    }

    void MouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -180f, 0f);

        body.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleSprint()
    {
        if (Input.GetKey(KeyCode.LeftShift) && sprintCounter > 0)
        {
            isSprinting = true;
            sprintCounter -= depleteRate * Time.deltaTime;
        }
        else
        {
            isSprinting = false;
            if (sprintCounter < maxSprint)
                sprintCounter += regenRate * Time.deltaTime;
        }

        // Clamp to ensure it doesn't go below 0
        sprintCounter = Mathf.Clamp(sprintCounter, 0, maxSprint);
    }
    public void TiltOnDamage(float tiltAmount = 10f, float duration = 0.2f)
    {
        // pick random side: left (-tilt) or right (+tilt)
        float randomTilt = Random.value < 0.5f ? -tiltAmount : tiltAmount;

        // start coroutine to tilt and reset
        StartCoroutine(TiltRoutine(randomTilt, duration));
    }

    private IEnumerator TiltRoutine(float angle, float duration)
    {
        Quaternion startRot = body.localRotation;
        Quaternion tiltRot = startRot * Quaternion.Euler(0f, 0f, angle);

        // tilt quickly
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            body.localRotation = Quaternion.Slerp(startRot, tiltRot, t);
            yield return null;
        }

        // return to normal
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            body.localRotation = Quaternion.Slerp(tiltRot, startRot, t);
            yield return null;
        }

        body.localRotation = startRot;
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
