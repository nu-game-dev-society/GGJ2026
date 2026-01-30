using UnityEngine;
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

    [Header("Required components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private PlayerInput input;
    #endregion

    private Vector2 moveInput;
    private Vector3 velocity;

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
    #endregion

    #region Unity Methods
 
    private void OnEnable()
    {
        input.actions[moveInputActionName].performed += OnMovePerformed;
        input.actions[moveInputActionName].canceled += OnMoveCancelled;

        input.actions[jumpInputActionName].performed += OnJumpPerformed;
    }

    private void OnDisable()
    {
        input.actions[moveInputActionName].performed -= OnMovePerformed;
        input.actions[moveInputActionName].canceled -= OnMoveCancelled;

        input.actions[jumpInputActionName].performed -= OnJumpPerformed;
    }

    void OnPlayerJoined(PlayerInput player)
    {
        Debug.Log(player);
        if (player.playerIndex == 0)
            player.SwitchCurrentControlScheme("KeyboardLeft", Keyboard.current);
        else
            player.SwitchCurrentControlScheme("KeyboardRight", Keyboard.current);
    }

    private void Update()
    {
        Move();
        ApplyGravity();
    }
    #endregion

    private void Move()
    {
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(moveSpeed * Time.deltaTime * move);
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
    }
}
