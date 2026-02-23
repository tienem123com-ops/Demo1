using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine ctx, PlayerStateFactory factory)
        : base(ctx, factory) { IsRootState = true; }

    public override void EnterState() { InitializeSubState(); }
    protected override void UpdateState() { CheckSwitchState(); }
    protected override void ExitState() { }

    public override void InitializeSubState()
    {
        if (_ctx.InputVector.magnitude < 0.01f) SetChildState(_factory.Idle());
        else SetChildState(_factory.Run());
    }

    public override void CheckSwitchState()
    {
        if (_ctx.JumpBufferCounter > 0 && _ctx.CoyoteCounter > 0) SwitchState(_factory.Jumping());
        else if (!_ctx.CharController.isGrounded) SwitchState(_factory.Falling());
    }
}