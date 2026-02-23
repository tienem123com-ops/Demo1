using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float _timer;
    private Vector3 _dashDir;

    public PlayerDashState(PlayerStateMachine ctx, PlayerStateFactory factory)
        : base(ctx, factory) { IsRootState = true; }

    public override void EnterState()
    {
        _timer = _ctx.dashDuration;

        // Lấy hướng ngang từ Camera (Triệt tiêu Y)
        _dashDir = _ctx.GetHorizontalDashDirection();

        // Xoay người ngay lập tức về hướng lướt
        if (_dashDir != Vector3.zero)
            _ctx.transform.rotation = Quaternion.LookRotation(_dashDir);

        _ctx.PlayAnimation("Dash", 0.05f);
        _ctx.ResetDashCooldown();
    }

    protected override void UpdateState()
    {
        // Chỉ lướt ngang
        _ctx.Velocity.x = _dashDir.x * _ctx.dashForce;
        _ctx.Velocity.z = _dashDir.z * _ctx.dashForce;

        // Chống bay lên khi vấp dốc
        if (_ctx.CharController.isGrounded) _ctx.Velocity.y = -2f;

        _timer -= Time.deltaTime;
        CheckSwitchState();
    }

    protected override void ExitState()
    {
        // Để lại 50% đà quán tính
        _ctx.Velocity.x *= 0.5f;
        _ctx.Velocity.z *= 0.5f;
    }

    public override void CheckSwitchState()
    {
        if (_timer <= 0)
        {
            if (_ctx.CharController.isGrounded) SwitchState(_factory.Grounded());
            else SwitchState(_factory.Falling());
        }
    }

    public override void InitializeSubState() { }
}