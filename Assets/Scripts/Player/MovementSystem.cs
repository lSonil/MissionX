using System.Collections;
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
    public Transform body;
    private float xRotation = 0f;

    [Header("Crouch Settings")]
    public float crouchHeight = .5f;
    public float standingHeight = 1f;

    public CharacterController controller;
    private Vector3 velocity;
    private bool isGrounded;
    private bool inAir=false;
    private bool isCrouching = false;
    public bool isBlocked = false;
    private Coroutine footstepRoutine;
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

        if(Input.GetKeyDown(KeyCode.Keypad1))
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
        // Ground check
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

        // Get input
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");
        if ((moveX != 0 || moveZ != 0) && footstepRoutine == null && isGrounded)
        {
            footstepRoutine = StartCoroutine(FootstepLoop());
        }

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        float currentSpeed = isCrouching && isGrounded ? crouchSpeed : walkSpeed;
        controller.Move(move * currentSpeed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && isGrounded && !isCrouching)
        {
            inAir = true;
            GetComponent<AudioSystem>().PlayJumpEffect();
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        // Apply gravity
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
            yield return new WaitForSeconds(0.3f);
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
