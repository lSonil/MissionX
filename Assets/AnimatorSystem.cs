using UnityEngine;

[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;
    private MovementSystem movement;
    private InventorySystem inventory;

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<MovementSystem>();
        inventory = GetComponent<InventorySystem>();
    }

    void Update()
    {
        if (movement == null || inventory == null) return;
        bool isMoving = GetMovementState();
        bool isHolding = GetHoldingState();
        bool isGrounded = movement.controller.isGrounded;
        bool isBlocked = Physics.Raycast(
            movement.transform.position + Vector3.up * 0.5f, // eye-level
            movement.transform.forward,
            0.2f,
            LayerMask.GetMask("Structure")
        );

        animator.SetBool("IsMoving", isMoving);
        animator.SetBool("IsHolding", isHolding);
        animator.SetBool("IsGrounded", isGrounded);
        if (isGrounded)
            animator.SetBool("IsInAir", false);

        if (isBlocked)
            animator.SetBool("IsInFront", true);
        else
        {
            animator.SetBool("IsBlocked", false);
            animator.SetBool("IsInFront", false);
        }
    }

    bool GetMovementState()
    {
        // Check if player is actively moving
        return !movement.isBlocked && (
            Input.GetAxisRaw("Horizontal") != 0 ||
            Input.GetAxisRaw("Vertical") != 0 ||
            !movement.controller.isGrounded ||
            movement.controller.velocity.magnitude > 0.1f
        );
    }

    bool GetHoldingState()
    {
        return inventory.GetHeldItem() != null;
    }
    public void SetIsInAir()
    {
        animator.SetBool("IsInAir", true);
    }
    public void SetIsBlocked()
    {
        animator.SetBool("IsBlocked", true);
    }
}
