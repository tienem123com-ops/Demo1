public class PlayerIdleState : PlayerBaseState
{
    public PlayerIdleState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState() { _ctx.PlayAnimation("Idle"); }
    protected override void UpdateState() { _ctx.Velocity.x = 0; _ctx.Velocity.z = 0; CheckSwitchState(); }
    protected override void ExitState() { }
    public override void CheckSwitchState()
    {
        if (_ctx.InputVector.magnitude > 0.01f) SwitchState(_factory.Run());
    }
    public override void InitializeSubState() { }
}