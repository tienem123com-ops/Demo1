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

        // Tính toán Speed Multiplier dựa trên hướng di chuyển khi đang ATK
        float speedMultiplier = 1.0f;
        if (_ctx.IsAttacking)
        {
            if (_ctx.InputVector.y < -0.1f) speedMultiplier = 0.65f; // Lùi chậm nhất
            else if (Mathf.Abs(_ctx.InputVector.y) < 0.1f) speedMultiplier = 0.8f; // Đi ngang vừa
        }

        float targetSpeed = _ctx.runMaxSpeed * speedMultiplier;

        // Áp dụng vận tốc mượt mà
        _ctx.Velocity.x = Mathf.Lerp(_ctx.Velocity.x, moveDir.x * targetSpeed, _ctx.runAcceleration * Time.deltaTime);
        _ctx.Velocity.z = Mathf.Lerp(_ctx.Velocity.z, moveDir.z * targetSpeed, _ctx.runAcceleration * Time.deltaTime);

        // Xử lý chuyển đổi Animation (Tránh dính frame bằng biến _currentAnimHash như đã làm)
        int targetAnim = _ctx.GetMovementAnimation();
        if (targetAnim != _currentAnimHash)
        {
            _currentAnimHash = targetAnim;
            _ctx.PlayAnimation(targetAnim, 0.15f);
        }

        CheckSwitchState();
    }
    public override void CheckSwitchState()
    {
        if (_ctx.InputVector.magnitude < 0.01f) SwitchState(_factory.Idle());
    }

    protected override void ExitState() { }
    public override void InitializeSubState() { }
}