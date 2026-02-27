using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    private const float LANDING_VELOCITY_THRESHOLD = -5.0f;

    public PlayerGroundedState(PlayerStateMachine ctx, PlayerStateFactory factory)
        : base(ctx, factory) { _isRootState = true; }

    public override void EnterState()
    {
        if (_ctx.Velocity.y < LANDING_VELOCITY_THRESHOLD)
            _ctx.PlayAnimation(_ctx.Anim_Land, 0.1f);

        InitializeSubState();
    }

    protected override void UpdateState() => CheckSwitchState();
    protected override void ExitState() { }

    public override void InitializeSubState()
    {
        if (_ctx.InputVector.magnitude < 0.01f)
            SetChildState(_factory.Idle());
        else
            SetChildState(_factory.Run());
    }

    public override void CheckSwitchState()
    {
        // Dash dưới đất
        if (Input.GetKeyDown(KeyCode.LeftShift) && _ctx.CanDash())
        {
            SwitchState(_factory.Dash());
            return;
        }

        // Jump với coyote + jump buffer
        if (_ctx.JumpBufferCounter > 0f && _ctx.CoyoteCounter > 0f)
        {
            SwitchState(_factory.Jumping());
            return;
        }

        // Leave ground
        if (!_ctx.CharController.isGrounded)
            SwitchState(_factory.Falling());
    }
}