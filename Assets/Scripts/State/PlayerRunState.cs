using UnityEngine;

public class PlayerRunState : PlayerBaseState
{
    public PlayerRunState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState() { _ctx.PlayAnimation("Run"); }

    protected override void UpdateState()
    {
        // Lấy hướng di chuyển tổ hợp (Phím + Camera)
        Vector3 moveDir = _ctx.GetLookDirection();

        // Áp dụng gia tốc và vận tốc vào Velocity
        // (Sử dụng runMaxSpeed từ hằng số GDC của bạn)
        _ctx.Velocity.x = moveDir.x * _ctx.runMaxSpeed;
        _ctx.Velocity.z = moveDir.z * _ctx.runMaxSpeed;

        CheckSwitchState();
    }

    public override void CheckSwitchState()
    {
        if (_ctx.InputVector.magnitude < 0.01f) SwitchState(_factory.Idle());
    }

    protected override void ExitState() { }
    public override void InitializeSubState() { }
}