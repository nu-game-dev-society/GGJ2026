using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{
    #region Serialised Fields
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Input actions")]
    [SerializeField] private string moveInputActionName = "Move";
    [SerializeField] private string jumpInputActionName = "Jump";
    [SerializeField] private string attackInputActionName = "Attack";

    [Header("Attack Controllers")]
    [SerializeField] private List<AttackController> attackControllers = new();
    private Dictionary<AttackType, AttackController> attackControllersKeyedByMask = new();
    private AttackController activeAttackController;

    [Header("Soft-Required components")]
    [SerializeField] private Camera cameraCached;
    [SerializeField] Animator animator;

    [Header("Hard-Required Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerInput input;
    #endregion

    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector3 externalForce; // Forces from rollers, conveyor belts, etc.

    #region Input callbacks
    private void OnMovePerformed(CallbackContext ctx)
    {
        moveInput = ctx.ReadValue<Vector2>();
    }

    private void OnMoveCancelled(CallbackContext ctx)
    {
        moveInput = Vector2.zero;
    }

    private void OnJumpPerformed(CallbackContext ctx)
    {
        Jump();
    }

    private void OnAttackPerformed(CallbackContext ctx)
    {
        Attack();
    }
    #endregion

    #region Unity Methods

    private void Awake()
    {
        cameraCached = Camera.main;

        foreach (var attackController in attackControllers)
        {
            attackControllersKeyedByMask.Add(attackController.AttackType, attackController);
        }
    }
 
    private void OnEnable()
    {
        input.actions[moveInputActionName].performed += OnMovePerformed;
        input.actions[moveInputActionName].canceled += OnMoveCancelled;

        input.actions[jumpInputActionName].performed += OnJumpPerformed;
        input.actions[attackInputActionName].performed += OnAttackPerformed;
    }

    private void OnDisable()
    {
        input.actions[moveInputActionName].performed -= OnMovePerformed;
        input.actions[moveInputActionName].canceled -= OnMoveCancelled;

        input.actions[jumpInputActionName].performed -= OnJumpPerformed;
        input.actions[attackInputActionName].performed -= OnAttackPerformed;
    }

    private void Update()
    {
        Move();
        ApplyGravity();
        ApplyExternalForces();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Check if we hit a roller pusher
        RollerPusher roller = hit.gameObject.GetComponent<RollerPusher>();
        if (roller != null)
        {
            // Get push force from the roller based on contact point
            Vector3 pushForce = roller.GetPushForce(hit.point);
            ApplyExternalForce(pushForce);
        }
    }
    #endregion

    private void Move()
    {
        // Get camera relative directions
        Vector3 camForward = cameraCached.transform.forward;
        Vector3 camRight = cameraCached.transform.right;

        // Flatten camera directions to ignore vertical tilt
        camForward.y = 0f;
        camRight.y = 0f;
        camForward.Normalize();
        camRight.Normalize();

        // Calculate movement relative to camera
        Vector3 move = camRight * moveInput.x + camForward * moveInput.y;
        move.Normalize();

        // Rotate the player to face movement direction
        if (move.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(move),
                10f * Time.deltaTime // rotation speed
            );
        }

        controller.Move(moveSpeed * Time.deltaTime * move);

        animator.SetFloat("Move", move.magnitude);
    }

    private void Attack()
    {
        if (this.activeAttackController == null)
        {
            Debug.LogError($"Failed to attack - {nameof(activeAttackController)} was null!");
            return;
        }
        this.activeAttackController.PerformAttack();
    }

    private void Jump()
    {
        if (controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        animator.SetFloat("YVel", controller.velocity.y);
    }

    public void OnMaskEquipped(MaskData mask)
    {
        if (this.activeAttackController != null)
        {
            this.activeAttackController.enabled = false;
        }

        if (this.attackControllersKeyedByMask.TryGetValue(mask.attackType, out this.activeAttackController))
        {
            this.activeAttackController.enabled = true;
        }
        else
        {
            Debug.LogError($"Failed to find attack controller for {mask.attackType}");
        }
    }

    /// <summary>
    /// Apply external force from rollers, conveyor belts, etc.
    /// </summary>
    public void ApplyExternalForce(Vector3 force)
    {
        externalForce = force;
    }

    private void ApplyExternalForces()
    {
        if (externalForce.magnitude > 0.01f)
        {
            // Apply the external force
            controller.Move(externalForce * Time.deltaTime);

            // Decay the force quickly so it doesn't accumulate
            // This allows player to fight against it
            externalForce = Vector3.Lerp(externalForce, Vector3.zero, 5f * Time.deltaTime);
        }
        else
        {
            externalForce = Vector3.zero;
        }
    }

    /// <summary>
    /// Called when player dies (e.g., falls into death zone)
    /// </summary>
    public void Kill()
    {
        Debug.Log($"Player {name} died!");

        // TODO: Implement proper death/respawn/elimination system
        // For now, just disable the player
        gameObject.SetActive(false);
    }
}
