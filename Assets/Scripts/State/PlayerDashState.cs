using UnityEngine;

public class PlayerDashState : PlayerBaseState
{
    private float _startTime;
    private Vector3 _dashDirection;

    // Tùy chỉnh Curve trong Inspector của PlayerController: 
    // Trục X từ 0->1, Trục Y từ 1->0 (giảm dần)
    private AnimationCurve _dashCurve;

    public PlayerDashState(PlayerStateMachine ctx, PlayerStateFactory factory)
       : base(ctx, factory) { IsRootState = true; }

    public override void EnterState()
    {
        _startTime = Time.time;
        _ctx.IsDashing = true; // Flag để khóa các logic khác nếu cần

        _ctx.PlayAnimation(_ctx.Anim_Dash, 0.05f);

        // Xác định hướng lướt chuẩn GDC: Ưu tiên hướng Camera + Input
        _dashDirection = _ctx.GetLookDirection();
        if (_dashDirection == Vector3.zero)
            _dashDirection = _ctx.model.forward;

        // Xoay nhân vật ngay lập tức về hướng lướt
        _ctx.model.rotation = Quaternion.LookRotation(_dashDirection);
    }

    protected override void UpdateState()
    {
        float elapsedTime = Time.time - _startTime;
        float percentComplete = elapsedTime / _ctx.dashDuration;

        if (percentComplete <= 1.0f)
        {
            // Tính toán vận tốc dựa trên quãng đường chia thời gian
            float speed = _ctx.dashLength / _ctx.dashDuration;

            // Áp dụng Curve để mượt hơn (giảm tốc về cuối)
            // Nếu không dùng Curve, bạn có thể dùng: speed * (1 - percentComplete)
            float curveMultiplier = _ctx.dashCurve.Evaluate(percentComplete);

            _ctx.Velocity.x = _dashDirection.x * speed * curveMultiplier;
            _ctx.Velocity.z = _dashDirection.z * speed * curveMultiplier;
            _ctx.Velocity.y = 0; // Khóa trục Y để lướt trên không chuẩn Romeo
        }

        CheckSwitchState();
    }

    public override void CheckSwitchState()
    {
        if (Time.time - _startTime >= _ctx.dashDuration)
        {
            _ctx.IsDashing = false;

            if (_ctx.CharController.isGrounded)
                SwitchState(_factory.Grounded());
            else
                SwitchState(_factory.Falling());
        }
    }
}