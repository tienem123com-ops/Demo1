using UnityEngine;

public class PlayerJumpingState : PlayerBaseState
{
    public PlayerJumpingState(PlayerStateMachine ctx, PlayerStateFactory factory)
        : base(ctx, factory) { IsRootState = true; }

    public override void EnterState()
    {
        _ctx.PlayAnimation(_ctx.Anim_Jump_Begin, 0.05f);
        ApplyJumpVelocity();
    }

    private void ApplyJumpVelocity()
    {
        _ctx.SetVelocity(
            _ctx.Velocity.x,
            _ctx.initialJumpVelocity,
            _ctx.Velocity.z
        );

        _ctx.JumpBufferCounter = 0f;
        _ctx.CoyoteCounter = 0f;
    }

    protected override void UpdateState()
    {
        // Immediately transition to falling after applying jump velocity
        CheckSwitchState();
    }

    protected override void ExitState() { }

    public override void CheckSwitchState()
    {
        SwitchState(_factory.Falling());
    }

    public override void InitializeSubState() { }
}