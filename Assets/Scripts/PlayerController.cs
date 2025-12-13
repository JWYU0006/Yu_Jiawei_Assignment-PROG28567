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

    public float gravity;
    public float jumpVel;

    private Vector2 playerInput;
    public bool jumpPressed;
    public bool jumpDuration;

    [Header("Variable Jump Height")]
    public float jumpHoldingTime;
    public float jumpHoldingThreshold;
    public bool jumpHolding;
    public float apexHeightMultiplier;
    public bool highJump;

    [Header("Dash")]
    public Vector2 currentDirection;
    public bool dashPressed;
    public float dashSpeed;
    public float dashDuration;
    public float dashTime;
    public bool dashing;
    public float dashCoolDown;
    public GameObject coolDownBar;
    public GameObject coolDownTime;

    [Header("Enemy")]
    public GameObject enemy;

    void Start()
    {
        acceleration = maxSpeed / accelerationTime;
        deceleration = maxSpeed / decelerationTime;

        // TODO: calculate gravity and jump velocity using the formulas from class
        body2D.gravityScale = 0;

        currentDirection = Vector2.right;
    }

    void Update()
    {
        gravity = -2 * apexHeight / (apexTime * apexTime);
        jumpVel = 2 * apexHeight * apexHeightMultiplier / apexTime;
        playerInput = new()
        {
            x = Input.GetAxisRaw("Horizontal"),
            y = Input.GetKeyDown(KeyCode.W) ? 1 : 0
        };
        if (IsGrounded()) fallingTime = 0; else fallingTime += Time.deltaTime;
        fallingTime = Mathf.Min(fallingTime, 2 * coyoteTime);
        if (playerInput.y == 1 && fallingTime <= coyoteTime && !jumpDuration && !dashing)
        {
            jumpPressed = true; jumpHolding = true;
        }
        if (Input.GetKeyUp(KeyCode.W)) { jumpHolding = false; }
        if (jumpHolding && !IsGrounded()) { jumpHoldingTime += Time.deltaTime; }

        if (Input.GetKeyDown(KeyCode.Space) && dashCoolDown <= 0) dashPressed = true;
        if (dashPressed) { dashCoolDown = 2; }
        dashCoolDown -= Time.deltaTime;
        dashCoolDown = Mathf.Max(dashCoolDown, 0);
        if (dashing) dashTime += Time.deltaTime; else dashTime = 0;
        coolDownTime.transform.localScale = new Vector2(Mathf.Lerp(1, 0, dashCoolDown / 2), 1);
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
        if (!dashing)
        {
            ProcessWalkInput();
            ProcessJumpInput();
        }

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

            currentDirection.x = playerInput.x;
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
        if (!highJump && jumpDuration && jumpHoldingTime >= jumpHoldingThreshold)
        {
            jumpPressed = true; highJump = true; apexHeightMultiplier = 1;
        }
        if (IsGrounded() && velocity.y < 0)
        {
            velocity.y = 0; jumpDuration = false; highJump = false; jumpHoldingTime = 0; apexHeightMultiplier = 0.7f;
        }
        else if (jumpPressed)
        {
            //if (highJump) { apexHeightMultiplier = 1; Debug.Log("1"); }
            //else { apexHeightMultiplier = 0.7f; Debug.Log("0.7"); }
            velocity.y = jumpVel;
            jumpDuration = true; jumpPressed = false;
        }
        else if (!IsGrounded()) { velocity.y += 0.5f * gravity * Time.fixedDeltaTime; }
        velocity.y = Mathf.Max(velocity.y, terminalSpeed);
    }
    private void ProcessDashInput()
    {
        if (dashPressed) { velocity = currentDirection * dashSpeed; dashing = true; dashPressed = false; }
        if (dashing && dashTime >= dashDuration) { velocity = Vector2.zero; dashing = false; }
        else if (dashing) { velocity = currentDirection * dashSpeed; }
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name == "Capsule")
        {
            if (!dashing)
            {
                Debug.Log("Die");
            }
            else
            {
                Destroy(collision.gameObject);
            }
        }
    }
}
