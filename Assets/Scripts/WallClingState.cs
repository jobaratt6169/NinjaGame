using UnityEngine;

public class WallClingState : PlayerBaseState
{
    private float slideSpeed = 1.5f; // Adjust this value for desired slide speed
    private float enterTime;
    private bool jumpHeldOnEnter;
    private float wallClingCooldown = 0.2f;
    private float lastWallClingTime = -1f;
    private bool isClinging = true; // Track if we're currently clinging

    public WallClingState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        enterTime = Time.time;
        jumpHeldOnEnter = stateMachine.InputReader.IsJumpPressed();
        lastWallClingTime = Time.time;
        isClinging = true; // Start clinging when entering state
        Debug.Log($"[WallClingState] Entering Wall Cling State at {enterTime:F2}s");

        // Play wall cling animation if available
        if (stateMachine.Animator != null)
            stateMachine.Animator.Play("WallCling");

        // Reduce initial vertical velocity slightly to make the cling feel better
        if (stateMachine.RB != null)
        {
            stateMachine.RB.linearVelocity = new Vector2(stateMachine.RB.linearVelocity.x, Mathf.Clamp(stateMachine.RB.linearVelocity.y, -slideSpeed, float.MaxValue));
        }
    }

    public override void Tick(float deltaTime)
    {
        // Check for Shoot input first
        if (stateMachine.InputReader.IsShootPressed())
        {
            stateMachine.SwitchState(stateMachine.ShootState);
            return;
        }

        // Check if C key is pressed to toggle wall cling
        if (stateMachine.InputReader.IsCrouchHeld() && Time.time - lastWallClingTime >= wallClingCooldown)
        {
            isClinging = !isClinging; // Toggle clinging state
            lastWallClingTime = Time.time;
            
            if (!isClinging)
            {
                stateMachine.SwitchState(stateMachine.FallState);
                return;
            }
        }

        // Only apply wall cling behavior if we're actually clinging
        if (isClinging)
        {
            // Apply slow downward slide
            if (stateMachine.RB != null)
            {
                // Apply a constant downward velocity, overriding gravity effect while clinging
                stateMachine.RB.linearVelocity = new Vector2(stateMachine.RB.linearVelocity.x, -slideSpeed);
            }

            // Check for jump input to perform a wall jump
            // Only allow wall jump if jump wasn't held on enter (prevents infinite wall jumps)
            if (stateMachine.InputReader.IsJumpPressed() && !jumpHeldOnEnter && Time.time - lastWallClingTime >= wallClingCooldown)
            {
                // Transition to JumpState, which should handle the wall jump logic
                stateMachine.SwitchState(stateMachine.JumpState);
                return;
            }
            // Update lockout: if jump is released, allow wall jump again
            if (!stateMachine.InputReader.IsJumpPressed())
            {
                jumpHeldOnEnter = false;
            }
        }
        else
        {
            // If not clinging, let gravity take effect
            if (stateMachine.RB != null)
            {
                stateMachine.RB.linearVelocity = new Vector2(stateMachine.RB.linearVelocity.x, stateMachine.RB.linearVelocity.y);
            }
        }

        // Check if grounded
        if (stateMachine.IsGrounded())
        {
            // Always reset jumps when grounded
            stateMachine.JumpsRemaining = stateMachine.MaxJumps;
            Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
            if (moveInput == Vector2.zero)
                stateMachine.SwitchState(stateMachine.IdleState);
            else if (stateMachine.InputReader.IsRunPressed())
                stateMachine.SwitchState(stateMachine.RunState);
            else
                stateMachine.SwitchState(stateMachine.WalkState);
            return;
        }

        // Check if no longer touching the wall
        if (!stateMachine.IsTouchingWall())
        {
            // Transition to FallState when losing wall contact
            stateMachine.SwitchState(stateMachine.FallState);
            return;
        }

        // Optional: Check if player moves away from the wall
        // Vector2 moveInput = stateMachine.GetMovementInput();
        // float wallDirection = stateMachine.GetWallDirection(); // Need a method to determine wall side (-1 left, 1 right)
        // if ((wallDirection < 0 && moveInput.x > 0) || (wallDirection > 0 && moveInput.x < 0))
        // {
        //     stateMachine.SwitchState(stateMachine.JumpState); // Detach from wall
        //     return;
        // }
    }

    public override void Exit()
    {
        Debug.Log($"[WallClingState] Exiting Wall Cling State after {Time.time - enterTime:F2}s");
        // Reset gravity if it was modified, or ensure velocity isn't stuck at slideSpeed
        // The JumpState or other subsequent states should handle setting appropriate velocities.
    }
}