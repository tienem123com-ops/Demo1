using UnityEngine;

public class PlayerFallingState : PlayerBaseState
{
    public PlayerFallingState(PlayerStateMachine ctx, PlayerStateFactory factory)
        : base(ctx, factory) { IsRootState = true; }

    public override void EnterState()
    {
        // Nếu chuyển từ Jump sang Falling, dùng transition mượt để thấy tư thế rơi
        _ctx.PlayAnimation(_ctx.Anim_Falling, 0.15f);
    }
    protected override void UpdateState()
    {
        Vector3 moveDir = _ctx.GetLookDirection();
        Vector3 targetMove = moveDir * _ctx.runMaxSpeed;

        // MOMENTUM: Sử dụng Lerp để vận tốc ngang thay đổi từ từ
        // Nếu vận tốc hiện tại đang nhanh hơn runMaxSpeed (do lướt rồi nhảy), nó sẽ giảm dần về maxSpeed
        _ctx.Velocity.x = Mathf.Lerp(_ctx.Velocity.x, targetMove.x, _ctx.airControl * Time.deltaTime);
        _ctx.Velocity.z = Mathf.Lerp(_ctx.Velocity.z, targetMove.z, _ctx.airControl * Time.deltaTime);

        CheckSwitchState();
    }

    public override void CheckSwitchState()
    {
        if (_ctx.CharController.isGrounded)
            SwitchState(_factory.Grounded());

        if (Input.GetKeyDown(KeyCode.LeftShift) && _ctx.CanDash())
            SwitchState(_factory.Dash());
    }

    protected override void ExitState() { }
    public override void InitializeSubState() { }
}