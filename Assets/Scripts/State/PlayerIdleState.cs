public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        // Gọi thông qua Hash đã khai báo ở Controller
        _ctx.PlayAnimation(_ctx.Anim_Idle, 0.1f);
    }
    protected override void UpdateState() { _ctx.Velocity.x = 0; _ctx.Velocity.z = 0; CheckSwitchState(); }
    protected override void ExitState() { }
    public override void CheckSwitchState()
    {
        if (_ctx.InputVector.magnitude > 0.01f) SwitchState(_factory.Run());
    }
    public override void InitializeSubState() { }
}