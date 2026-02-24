using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerRunState : PlayerBaseState
{
    private int _lastAnimHash;

    public PlayerRunState(PlayerStateMachine ctx, PlayerStateFactory factory) : base(ctx, factory) { }

    public override void EnterState()
    {
        // Đừng gọi string "Run", hãy gọi hàm xác định 8 hướng của bạn
        int moveAnim = _ctx.GetMovementAnimation();
        _ctx.PlayAnimation(moveAnim, 0.1f);
    }
    private int _currentAnimHash; // Lưu hash đang chơi để tránh gọi lại liên tục

    protected override void UpdateState()
    {
        Vector3 moveDir = _ctx.GetLookDirection();
        float targetSpeed = _ctx.runMaxSpeed;

        // Tính toán modifier nếu đang tấn công
        if (_ctx.IsAttacking)
        {
            if (_ctx.InputVector.y < -0.1f) targetSpeed *= 0.65f;
            else if (Mathf.Abs(_ctx.InputVector.y) < 0.1f) targetSpeed *= 0.8f;
        }

        // Gán vận tốc di chuyển ngang
        _ctx.Velocity.x = moveDir.x * targetSpeed;
        _ctx.Velocity.z = moveDir.z * targetSpeed;

        // Cập nhật Animation
        _ctx.PlayAnimation(_ctx.GetMovementAnimation());

        CheckSwitchState();
    }
    public override void CheckSwitchState()
    {
        if (_ctx.InputVector.magnitude < 0.01f) SwitchState(_factory.Idle());
    }

    protected override void ExitState() { }
    public override void InitializeSubState() { }
}