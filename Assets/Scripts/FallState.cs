using UnityEngine;

public class FallState : PlayerBaseState
{
    private float enterTime;
    private float jumpCooldown = 0.2f; // Match the cooldown from JumpState
    private float lastJumpTime = -1f;
    private float wallClingCooldown = 0.2f;
    private float lastWallClingTime = -1f;

    public FallState(PlayerStateMachine stateMachine) : base(stateMachine) { }

    public override void Enter()
    {
        enterTime = Time.time;
        // Play fall animation if available
        if (stateMachine.Animator != null)
            stateMachine.Animator.Play("Fall");
        Debug.Log($"[FallState] Entering Fall State at {enterTime:F2}s");
    }

    public override void Tick(float deltaTime)
    {
        // Only allow horizontal movement during fall
        Vector2 moveInput = stateMachine.InputReader.GetMovementInput();
        float targetVelocityX = moveInput.x * stateMachine.MoveSpeed;
        if (stateMachine.RB != null)
        {
            // Only modify horizontal velocity, let gravity handle vertical movement
            stateMachine.RB.linearVelocity = new Vector2(targetVelocityX, stateMachine.RB.linearVelocity.y);
        }

        // Check for wall cling input
        if (stateMachine.InputReader.IsCrouchHeld() && 
            stateMachine.IsTouchingWall() && 
            Time.time - lastWallClingTime >= wallClingCooldown)
        {
            stateMachine.SwitchState(stateMachine.WallClingState);
            return;
        }

        // If grounded, transition to Idle/Walk/Run
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

        // If touching wall and falling, transition to WallClingState
        if (stateMachine.IsTouchingWall() && stateMachine.RB.linearVelocity.y <= 0)
        {
            stateMachine.SwitchState(stateMachine.WallClingState);
            return;
        }

        // Allow jump if jumps remain and cooldown has passed
        if (stateMachine.InputReader.IsJumpPressed() && 
            stateMachine.JumpsRemaining > 0 && 
            Time.time - lastJumpTime >= jumpCooldown)
        {
            stateMachine.SwitchState(stateMachine.JumpState);
            return;
        }

        // Allow shooting in air
        if (stateMachine.InputReader.IsShootPressed())
        {
            stateMachine.SwitchState(stateMachine.ShootState);
            return;
        }
    }

    public override void Exit()
    {
        Debug.Log($"[FallState] Exiting Fall State after {Time.time - enterTime:F2}s");
    }
}