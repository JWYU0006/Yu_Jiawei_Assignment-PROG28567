using UnityEngine;

public class PlayerController : MonoBehaviour
{
    // Facing direction for movement and dash
    public enum FacingDirection
    {
        left = -1, right = 1
    }

    // Basic character states
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
    public float apexHeight;              // Jump height
    public float apexTime;                // Time to reach jump apex
    public float terminalSpeed;            // Max falling speed
    public float coyoteTime;               // Grace time after leaving ground
    public float fallingTime;              // Time since leaving ground
    public LayerMask groundLayer;
    public float groundCheckDistance;
    public Vector2 groundCheckSize;

    public Vector2 velocity;
    private float acceleration;
    private float deceleration;

    public float gravity;                  // Calculated gravity value
    public float jumpVel;                  // Calculated jump velocity

    private Vector2 playerInput;
    public bool jumpPressed;               // Jump input trigger
    public bool jumpDuration;              // Whether player is in jump state

    [Header("Variable Jump Height")]
    public float jumpHoldingTime;           // How long jump key is held
    public float jumpHoldingThreshold;      // Threshold for high jump
    public bool jumpHolding;                // Whether jump key is being held
    public float apexHeightMultiplier;      // Multiplier for jump height
    public bool highJump;                   // Whether high jump is triggered

    [Header("Dash")]
    public Vector2 currentDirection;        // Fixed dash direction
    public bool dashPressed;                // Dash input trigger
    public float dashSpeed;
    public float dashDuration;
    public float dashTime;                  // Current dash time
    public bool dashing;                    // Whether player is dashing
    public float dashCoolDown;              // Dash cooldown time
    public GameObject coolDownBar;          // Dash cooldown UI background
    public GameObject coolDownTime;         // Dash cooldown UI fill

    [Header("Enemy")]
    public GameObject enemy;

    void Start()
    {
        // Calculate acceleration and deceleration values
        acceleration = maxSpeed / accelerationTime;
        deceleration = maxSpeed / decelerationTime;

        // Disable built-in gravity, use custom gravity
        body2D.gravityScale = 0;

        // Default facing direction
        currentDirection = Vector2.right;
    }

    void Update()
    {
        // Calculate gravity and jump velocity every frame
        gravity = -2 * apexHeight / (apexTime * apexTime);
        jumpVel = 2 * apexHeight * apexHeightMultiplier / apexTime;

        // Get player input
        playerInput = new()
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetKeyDown(KeyCode.W) ? 1 : 0
        };

        // Update falling time for coyote time
        if (IsGrounded()) fallingTime = 0;
        else fallingTime += Time.deltaTime;

        fallingTime = Mathf.Min(fallingTime, 2 * coyoteTime);

        // Jump input check with coyote time and dash restriction
        if (playerInput.y == 1 && fallingTime <= coyoteTime && !jumpDuration && !dashing)
        {
            jumpPressed = true;
            jumpHolding = true;
        }

        // Stop jump holding when key is released
        if (Input.GetKeyUp(KeyCode.W))
        {
            jumpHolding = false;
        }

        // Track jump holding time while airborne
        if (jumpHolding && !IsGrounded())
        {
            jumpHoldingTime += Time.deltaTime;
        }

        // Dash input detection (must be in Update)
        if (Input.GetKeyDown(KeyCode.Space) && dashCoolDown <= 0)
            dashPressed = true;

        // Start dash cooldown
        if (dashPressed)
        {
            dashCoolDown = 2;
        }

        // Reduce dash cooldown over time
        dashCoolDown -= Time.deltaTime;
        dashCoolDown = Mathf.Max(dashCoolDown, 0);

        // Track dash duration
        if (dashing) dashTime += Time.deltaTime;
        else dashTime = 0;

        // Update cooldown UI scale
        coolDownTime.transform.localScale =
            new Vector2(Mathf.Lerp(1, 0, dashCoolDown / 2), 1);

        // Show or hide cooldown UI
        if (dashCoolDown == 0)
        {
            coolDownBar.GetComponent<SpriteRenderer>().enabled = false;
            coolDownTime.GetComponent<SpriteRenderer>().enabled = false;
        }
        else
        {
            coolDownBar.GetComponent<SpriteRenderer>().enabled = true;
            coolDownTime.GetComponent<SpriteRenderer>().enabled = true;
        }
    }

    private void FixedUpdate()
    {
        MovementUpdate();
        ProcessDashInput();
    }

    private void MovementUpdate()
    {
        // Disable normal movement while dashing
        if (!dashing)
        {
            ProcessWalkInput();
            ProcessJumpInput();
        }

        // Apply final velocity to Rigidbody
        body2D.linearVelocity = velocity;
    }

    /// <summary>
    /// Handles horizontal movement and acceleration.
    /// </summary>
    private void ProcessWalkInput()
    {
        if (playerInput.x != 0)
        {
            // Flip velocity when changing direction
            if (Mathf.Sign(playerInput.x) != Mathf.Sign(velocity.x))
                velocity.x *= -1;

            velocity.x += playerInput.x * acceleration * Time.fixedDeltaTime;
            velocity.x = Mathf.Clamp(velocity.x, -maxSpeed, maxSpeed);

            State = CharacterState.Walking;

            // Update facing direction
            currentDirection.x = playerInput.x;
        }
        else if (Mathf.Abs(velocity.x) > 0.005f)
        {
            // Decelerate when no input
            velocity.x += -Mathf.Sign(velocity.x) * deceleration * Time.fixedDeltaTime;
        }
        else
        {
            velocity.x = 0;
            State = CharacterState.Idle;
        }
    }

    /// <summary>
    /// Handles jumping, gravity, coyote time, and variable jump height.
    /// </summary>
    private void ProcessJumpInput()
    {
        // Trigger high jump if holding jump long enough
        if (!highJump && jumpDuration && jumpHoldingTime >= jumpHoldingThreshold)
        {
            jumpPressed = true;
            highJump = true;
            apexHeightMultiplier = 1;
        }

        // Reset jump state when grounded
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = 0;
            jumpDuration = false;
            highJump = false;
            jumpHoldingTime = 0;
            apexHeightMultiplier = 0.7f;
        }
        // Apply jump velocity
        else if (jumpPressed)
        {
            velocity.y = jumpVel;
            jumpDuration = true;
            jumpPressed = false;
        }
        // Apply gravity while airborne
        else if (!IsGrounded())
        {
            velocity.y += 0.5f * gravity * Time.fixedDeltaTime;
        }

        // Clamp falling speed
        velocity.y = Mathf.Max(velocity.y, terminalSpeed);
    }

    /// <summary>
    /// Handles dash movement and timing.
    /// </summary>
    private void ProcessDashInput()
    {
        // Start dash
        if (dashPressed)
        {
            velocity = currentDirection * dashSpeed;
            dashing = true;
            dashPressed = false;
        }

        // Stop dash when duration ends
        if (dashing && dashTime >= dashDuration)
        {
            velocity = Vector2.zero;
            dashing = false;
        }
        // Maintain dash velocity
        else if (dashing)
        {
            velocity = currentDirection * dashSpeed;
        }
    }

    public bool IsWalking()
    {
        return playerInput.x != 0;
    }

    // Ground check using OverlapBox
    public bool IsGrounded()
    {
        Vector3 origin = transform.position + Vector3.down * groundCheckDistance;
        DrawGroundCheck(origin);
        return Physics2D.OverlapBox(origin, groundCheckSize, 0, groundLayer);
    }

    // Debug visualization for ground check
    private void DrawGroundCheck(Vector3 origin)
    {
        float halfW = groundCheckSize.x * 0.5f;
        float halfH = groundCheckSize.y * 0.5f;

        Debug.DrawLine(origin + new Vector3(halfW, halfH), origin + new Vector3(halfW, -halfH), Color.yellow);
        Debug.DrawLine(origin + new Vector3(halfW, -halfH), origin + new Vector3(-halfW, -halfH), Color.yellow);
        Debug.DrawLine(origin + new Vector3(-halfW, -halfH), origin + new Vector3(-halfW, halfH), Color.yellow);
        Debug.DrawLine(origin + new Vector3(-halfW, halfH), origin + new Vector3(halfW, halfH), Color.yellow);
    }

    // Get current facing direction
    public FacingDirection GetFacingDirection()
    {
        return (FacingDirection)playerInput.x;
    }

    // Enemy collision handling
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Capsule")
        {
            if (!dashing)
            {
                // Player dies if not dashing
                Debug.Log("Die");
            }
            else
            {
                // Kill enemy while dashing
                Destroy(collision.gameObject);
            }
        }
    }
}
