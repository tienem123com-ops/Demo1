using UnityEngine;

public class PlayerJumpingState : PlayerBaseState
{
    public PlayerJumpingState(PlayerStateMachine ctx, PlayerStateFactory factory)
        : base(ctx, factory) { IsRootState = true; }

    public override void EnterState()
    {
        _ctx.PlayAnimation(_ctx.Anim_Jump_Begin, 0.05f); // Transition rất ngắn để bật lên ngay
        _ctx.Velocity.y = _ctx.initialJumpVelocity;
    }

    private void HandleJump()
    {
        // Tính toán lực nhảy dựa trên công thức vật lý mượt mà
        // Velocity.y = sqrt(jumpHeight * -2 * gravity)
        _ctx.Velocity.y = _ctx.initialJumpVelocity;

        // Reset Jump Buffer để tránh nhảy liên hồi
        _ctx.JumpBufferCounter = 0;
        _ctx.CoyoteCounter = 0;
    }

    protected override void UpdateState()
    {
        // Vừa nhảy xong là chuyển sang Falling ngay để Falling quản lý momentum
        CheckSwitchState();
    }

    protected override void ExitState() { }

    public override void CheckSwitchState()
    {
        // Luôn chuyển sang Falling sau khi đã áp dụng lực nhảy ban đầu
        SwitchState(_factory.Falling());
    }

    public override void InitializeSubState() { }
}