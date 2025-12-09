using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public enum FacingDirection
    {
        left = -1, right = 1
    }

    public enum CharacterState
    {
        Idle, Walking, Jumping, Dead
    }

    public CharacterState State { get; set; } = CharacterState.Idle;

    [SerializeField] private Rigidbody2D body2D;

    [Header("Walk Properties")]
    public float maxSpeed;
    public float accelerationTime;
    public float decelerationTime;

    [Header("Jump Properties")]
    public float apexHeight;
    public float apexTime;
    public float terminalSpeed;
    public float coyoteTime;
    public float fallingTime;
    public LayerMask groundLayer;
    public float groundCheckDistance;
    public Vector2 groundCheckSize;

    public Vector2 velocity;
    private float acceleration;
    private float deceleration;

    private float gravity;
    private float jumpVel;

    private Vector2 playerInput;
    public bool jumpPressed = false;
    public bool jumpDuration = false;

    void Start()
    {
        acceleration = maxSpeed / accelerationTime;
        deceleration = maxSpeed / decelerationTime;

        // TODO: calculate gravity and jump velocity using the formulas from class
        gravity = -2 * apexHeight / (apexTime * apexTime);
        jumpVel = 2 * apexHeight / apexTime;
        body2D.gravityScale = 0;
    }

    void Update()
    {
        playerInput = new()
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetKeyDown(KeyCode.W) ? 1 : 0
        };
        if (IsGrounded()) fallingTime = 0; else fallingTime += Time.deltaTime;
        fallingTime = Mathf.Min(fallingTime, 2 * coyoteTime);
        if (playerInput.y == 1 && fallingTime <= coyoteTime && !jumpDuration) jumpPressed = true;
    }

    private void FixedUpdate()
    {
        MovementUpdate();
    }

    private void MovementUpdate()
    {
        ProcessWalkInput();
        ProcessJumpInput();

        body2D.linearVelocity = velocity;
    }

    /// <summary>
    /// Modifies velocity.x based on playerInput.x.
    /// </summary>
    private void ProcessWalkInput()
    {
        if (playerInput.x != 0)
        {
            if (Mathf.Sign(playerInput.x) != Mathf.Sign(velocity.x)) velocity.x *= -1;
            velocity.x += playerInput.x * acceleration * Time.fixedDeltaTime;

            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);

            State = CharacterState.Walking;
        }
        else if (Mathf.Abs(velocity.x) > 0.005f)
        {
            velocity.x += -Mathf.Sign(velocity.x) * deceleration * Time.fixedDeltaTime;
        }
        else
        {
            velocity.x = 0;

            State = CharacterState.Idle;
        }
    }

    /// <summary>
    /// Modifies velocity.y based on playerInput.y.
    /// </summary>
    private void ProcessJumpInput()
    {
        if (IsGrounded() && velocity.y < 0) { velocity.y = 0; jumpDuration = false; }
        else if (IsGrounded() && jumpPressed && velocity.y == 0) { velocity.y = jumpVel; jumpDuration = true; jumpPressed = false; }
        else if (!IsGrounded()) { velocity.y += 0.5f * gravity * Time.fixedDeltaTime; }
        velocity.y = Mathf.Max(velocity.y, terminalSpeed);
    }

    public bool IsWalking()
    {
        return playerInput.x != 0;
    }
    public bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.down * groundCheckDistance;

        DrawGroundCheck(origin);

        return Physics2D.OverlapBox(origin, groundCheckSize, 0, groundLayer);
    }

    private void DrawGroundCheck(Vector3 origin)
    {
        float halfW = groundCheckSize.x * 0.5f;
        float halfH = groundCheckSize.y * 0.5f;

        Debug.DrawLine(origin + new Vector3(halfW, halfH), origin + new Vector3(halfW, -halfH), Color.yellow);
        Debug.DrawLine(origin + new Vector3(halfW, -halfH), origin + new Vector3(-halfW, -halfH), Color.yellow);
        Debug.DrawLine(origin + new Vector3(-halfW, -halfH), origin + new Vector3(-halfW, halfH), Color.yellow);
        Debug.DrawLine(origin + new Vector3(-halfW, halfH), origin + new Vector3(halfW, halfH), Color.yellow);
    }

    public FacingDirection GetFacingDirection()
    {
        return (FacingDirection)playerInput.x;
    }
}
