using UnityEngine;

public class JumpState : PlayerBaseState
{
    private float jumpForce = 7.5f;
    private float wallJumpHorizontalForce = 5f;
    private float enterTime;
    private bool hasJumped = false;
    private bool isWallJump = false;
    private float jumpCooldown = 0.2f; // Cooldown between jumps
    private float lastJumpTime = -1f; // Initialize to allow first jump

    public JumpState(PlayerStateMachine stateMachine) : base(stateMachine)
    {
    }

    public override void Enter()
    {
        enterTime = Time.time;
        hasJumped = false;
        isWallJump = false;

        // Check if we can jump based on cooldown
        if (Time.time - lastJumpTime < jumpCooldown)
        {
            // If on cooldown, transition to appropriate state
            if (!stateMachine.IsGrounded())
            {
                stateMachine.SwitchState(stateMachine.FallState);
            }
            else
            {
                stateMachine.SwitchState(stateMachine.IdleState);
            }
            return;
        }

        // --- Start coyote time (grounded grace period) ---
        stateMachine.GetType().GetField("jumpGroundedGraceTimer", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.SetValue(stateMachine, 0.10f);

        // Play jump animation
        if (stateMachine.Animator != null)
            stateMachine.Animator.Play("Jump");
        Debug.Log($"[JumpState] Entering Jump State at {enterTime:F2}s");

        // If grounded, set jumps to MaxJumps - 1 (so the ground jump counts as the first jump)
        bool isWallJumpNow = stateMachine.IsTouchingWall() && !stateMachine.IsGrounded();

        if (isWallJumpNow)
        {
            // Only allow wall jump if JumpsRemaining > 0
            if (stateMachine.JumpsRemaining > 0)
            {
                Vector2 wallJumpDir = GetWallJumpDirection();
                if (stateMachine.RB != null)
                {
                    // Reset velocity before applying wall jump force
                    stateMachine.RB.linearVelocity = Vector2.zero;
                    // Apply wall jump force with proper vertical component
                    stateMachine.RB.AddForce(new Vector2(wallJumpDir.x * wallJumpHorizontalForce, jumpForce), ForceMode2D.Impulse);
                    hasJumped = true;
                    isWallJump = true;
                    stateMachine.JumpsRemaining = Mathf.Max(0, stateMachine.JumpsRemaining - 1);
                    lastJumpTime = Time.time; // Set last jump time
                    Debug.Log("[JumpState] Wall Jump performed. Jumps left: " + stateMachine.JumpsRemaining);
                }
            }
        }
        else if (stateMachine.IsGrounded())
        {
            // Ground jump: do not decrement JumpsRemaining (already set to MaxJumps-1 on landing)
            if (stateMachine.RB != null)
            {
                // Reset vertical velocity before applying jump force
                stateMachine.RB.linearVelocity = new Vector2(stateMachine.RB.linearVelocity.x, 0f);
                // Apply pure vertical jump force
                stateMachine.RB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
                lastJumpTime = Time.time; // Set last jump time
                Debug.Log("[JumpState] Ground Jump performed. Jumps left: " + stateMachine.JumpsRemaining);
            }
        }
        else if (stateMachine.JumpsRemaining > 0)
        {
            // Air jump: decrement JumpsRemaining
            stateMachine.JumpsRemaining = Mathf.Max(0, stateMachine.JumpsRemaining - 1);
            if (stateMachine.RB != null)
            {
                // Reset vertical velocity before applying double jump force
                stateMachine.RB.linearVelocity = new Vector2(stateMachine.RB.linearVelocity.x, 0f);
                // Apply pure vertical double jump force
                stateMachine.RB.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
                hasJumped = true;
                lastJumpTime = Time.time; // Set last jump time
                Debug.Log("[JumpState] Double Jump performed. Jumps left: " + stateMachine.JumpsRemaining);
            }
        }
        else
        {
            // No jumps left, do nothing
            Debug.Log("[JumpState] No jumps remaining.");
        }

        // Play jump sound if needed
        // AudioManager.Instance?.Play("JumpSound");
    }

    public override void Tick(float deltaTime)
    {
        // Debug: Print grounded state and vertical velocity every frame in JumpState
        Debug.Log($"[JumpState] Tick: IsGrounded={stateMachine.IsGrounded()} VelocityY={stateMachine.RB.linearVelocity.y}");

        // Only allow horizontal movement during jump
        Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
        float targetVelocityX = moveInput.x * stateMachine.MoveSpeed;
        if (stateMachine.RB != null)
        {
            // Only modify horizontal velocity, let the jump force handle vertical movement
            stateMachine.RB.linearVelocity = new Vector2(targetVelocityX, stateMachine.RB.linearVelocity.y);
        }

        // Check for Shoot input first
        if (stateMachine.InputReader.IsShootPressed())
        {
            stateMachine.SwitchState(stateMachine.ShootState);
            return;
        }

        // If grounded, reset jumps and transition to Idle/Walk/Run
        if (stateMachine.IsGrounded())
        {
            stateMachine.JumpsRemaining = stateMachine.MaxJumps;
            if (moveInput == Vector2.zero)
                stateMachine.SwitchState(stateMachine.IdleState);
            else if (stateMachine.InputReader.IsRunPressed())
                stateMachine.SwitchState(stateMachine.RunState);
            else
                stateMachine.SwitchState(stateMachine.WalkState);
            return;
        }

        // Check if falling against a wall -> transition to Wall Cling
        if (stateMachine.IsTouchingWall() && stateMachine.RB.linearVelocity.y <= 0)
        {
            stateMachine.SwitchState(stateMachine.WallClingState);
            return;
        }

        // If not grounded and not touching wall, transition to FallState
        if (!stateMachine.IsGrounded() && !stateMachine.IsTouchingWall())
        {
            stateMachine.SwitchState(stateMachine.FallState);
            return;
        }
    }

    public override void Exit()
    {
        Debug.Log($"[JumpState] Exiting Jump State after {Time.time - enterTime:F2}s");
    }

    // Stub: returns direction away from wall (replace with actual wall normal logic)
    private Vector2 GetWallJumpDirection()
    {
        // Use facing direction to determine wall jump direction
        // If facing right (localScale.x > 0), wall is on right, so jump left; else jump right
        float facing = stateMachine.transform.localScale.x;
        return facing > 0 ? Vector2.left : Vector2.right;
    }
}