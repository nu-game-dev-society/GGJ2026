using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;

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
    [SerializeField] private Renderer markerRenderer;

    [Header("Particles")]
    [SerializeField] private ParticleSystem stunParticles;
    [SerializeField] private ParticleSystem hitParticles;


    [Header("SFX")]
    [SerializeField] private AudioSource hitSource;
    [SerializeField] private AudioClip hitClip;
    #endregion

    private Vector2 moveInput;
    private Vector3 velocity;
    private Vector3 externalForce; // Forces from rollers, conveyor belts, etc.
    private Vector3 knockbackVelocity; // One-time knockback impulses from attacks
    private RollerPusher currentRoller; // The roller we're currently touching

    private bool isStunned = false;
    private float stunEndTime = 0f;
    private Vector3 lastContactPoint; // Last point of contact with roller
    
    [HideInInspector]
    public Color playerColor;

    [Header("Events")]
    public UnityEvent OnPlayerKilled;

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

    private void Start()
    {
        markerRenderer.material = new Material(markerRenderer.material);
        markerRenderer.material.color = playerColor;
    }

    private void Update()
    {
        // Check if stun duration has expired
        if (isStunned && Time.time >= stunEndTime)
        {
            isStunned = false;
            stunParticles.Stop();
            animator.SetBool("Stunned", false);
        }

        // Clear roller reference at start of frame
        // It will be set again by OnControllerColliderHit if we're still touching
        currentRoller = null;

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
            // Store current roller and contact point
            currentRoller = roller;
            lastContactPoint = hit.point;
        }
    }
    #endregion

    private void Move()
    {
        // Don't allow movement while stunned
        if (isStunned)
        {
            animator.SetFloat("Move", 0f);
            return;
        }

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
        // Can't attack while stunned
        if (isStunned) return;

        if (this.activeAttackController == null)
        {
            Debug.LogError($"Failed to attack - {nameof(activeAttackController)} was null!");
            return;
        }
        this.activeAttackController.PerformAttack();
    }

    private void Jump()
    {
        // Can't jump while stunned
        if (isStunned) return;

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

    /// <summary>
    /// Apply knockback impulse from attacks
    /// </summary>
    public void ApplyKnockback(Vector3 knockbackVector)
    {
        // Add to existing knockback (allows multiple hits to stack)
        knockbackVelocity += knockbackVector;

        // Also add upward velocity if grounded
        if (controller.isGrounded && knockbackVector.y > 0)
        {
            velocity.y = knockbackVector.y;
        }
    }

    /// <summary>
    /// Stun the player for a duration (disables movement, jumping, attacking)
    /// Called via UnityEvent from PlayerHealthController when health reaches 0
    /// </summary>
    public void Stun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        stunParticles.Play();
        animator.SetBool("Stunned", true);
        Debug.Log($"{name} stunned for {duration} seconds");
    }

    private void ApplyExternalForces()
    {
        // Handle knockback (decays quickly)
        if (knockbackVelocity.magnitude > 0.01f)
        {
            controller.Move(knockbackVelocity * Time.deltaTime);
            // Decay knockback quickly (friction-like behavior)
            knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, 8f * Time.deltaTime);
        }

        // If we're on a roller, continuously get push force
        if (currentRoller != null)
        {
            externalForce = currentRoller.GetPushForce(lastContactPoint);
            // Apply the roller force
            controller.Move(externalForce * Time.deltaTime);
        }
        else
        {
            // Not on a roller anymore, quickly decay any remaining force
            if (externalForce.magnitude > 0.01f)
            {
                controller.Move(externalForce * Time.deltaTime);
                externalForce = Vector3.Lerp(externalForce, Vector3.zero, 10f * Time.deltaTime);
            }
        }
    }

    /// <summary>
    /// Called when player dies (e.g., falls into death zone)
    /// </summary>
    public void Kill()
    {
        Debug.Log($"Player {name} died!");

        OnPlayerKilled.Invoke();

        // TODO: Implement proper death/respawn/elimination system
        // For now, just disable the player
        gameObject.SetActive(false);
    }

    public void PlayHit()
    {
        hitParticles.Play();

        hitSource.clip = hitClip;
        hitSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);

        hitSource.Play();
    }
}
