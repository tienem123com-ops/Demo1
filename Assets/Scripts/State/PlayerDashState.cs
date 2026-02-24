using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float _timer;
    private Vector3 _dashVelocity;

    public PlayerDashState(PlayerStateMachine ctx, PlayerStateFactory factory)
     : base(ctx, factory) { IsRootState = true; }

    public override void EnterState()
    {
        _ctx.IsDashing = true;
        _ctx.PlayAnimation(_ctx.Anim_Dash, 0.05f);

        // Hướng Dash chuẩn
        Vector3 dashDir = _ctx.GetLookDirection();
        if (dashDir == Vector3.zero) dashDir = _ctx.model.forward;

        float speed = _ctx.dashLength / _ctx.dashDuration;
        _dashVelocity = dashDir * speed;
        _timer = 0;
    }

    protected override void UpdateState()
    {
        _timer += Time.deltaTime;

        _ctx.Velocity.x = _dashVelocity.x;
        _ctx.Velocity.z = _dashVelocity.z;
        _ctx.Velocity.y = 0; // Khóa trọng lực

        if (_timer >= _ctx.dashDuration)
        {
            CheckSwitchState();
        }
    }

    protected override void ExitState()
    {
        _ctx.IsDashing = false;
        // Celeste-style: Giữ lại một phần lực sau khi Dash để di chuyển tiếp mượt mà
        _ctx.Velocity.x *= 0.5f;
        _ctx.Velocity.z *= 0.5f;
    }

    public override void CheckSwitchState()
    {
        if (_ctx.CharController.isGrounded)
            SwitchState(_factory.Grounded());
        else
            SwitchState(_factory.Falling());
    }
}