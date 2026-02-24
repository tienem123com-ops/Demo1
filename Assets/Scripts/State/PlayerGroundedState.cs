using UnityEngine;

public class PlayerGroundedState : PlayerBaseState
{
    public PlayerGroundedState(PlayerStateMachine ctx, PlayerStateFactory factory)
        : base(ctx, factory) { IsRootState = true; }

    // Trong PlayerGroundedState.cs hoặc logic kiểm tra IsGrounded
    public override void EnterState()
    {
        // Nếu vận tốc rơi trước đó đủ lớn (vừa rơi xuống)
        if (_ctx.Velocity.y < -5.0f)
        {
            _ctx.PlayAnimation(_ctx.Anim_Land, 0.1f);
        }
        InitializeSubState();
    }
    protected override void UpdateState() { CheckSwitchState(); }
    protected override void ExitState() { }

    public override void InitializeSubState()
    {
        if (_ctx.InputVector.magnitude < 0.01f) SetChildState(_factory.Idle());
        else SetChildState(_factory.Run());
    }

    public override void CheckSwitchState()
    {
        // Lệnh này cực kỳ quan trọng để cho phép Dash dưới đất
        if (Input.GetKeyDown(KeyCode.LeftShift) && _ctx.CanDash())
        {
            SwitchState(_factory.Dash());
            return;
        }

        if (_ctx.JumpBufferCounter > 0 && _ctx.CoyoteCounter > 0) SwitchState(_factory.Jumping());
        else if (!_ctx.CharController.isGrounded) SwitchState(_factory.Falling());
    }
}