using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private ComboSequence _normalAttackCombo;
    private int _currentIndex = 0;
    
    private bool _comboQueued = false;
    private bool _hasDealtDamage = false;
private float _bufferTimer = 0f;
    private const float BUFFER_WINDOW = 0.2f; // Cho phép bấm trước 0.2s
    public PlayerAttackState(PlayerController ctx, PlayerStateFactory factory)
        : base(ctx, factory)
    {
        _normalAttackCombo =ctx._normalAttackCombo;
        _isRootState = true;
    }

    public override void EnterState()
    {
        _currentIndex = 0;
        StartAttackStep();
    }

    private void StartAttackStep()
    {
        _comboQueued = false;
        _hasDealtDamage = false;
        _ctx.SetAttackLock(true);
        _ctx.SetRotationLock(true);
        var data = _normalAttackCombo.attacks[_currentIndex];
        _ctx.PlayAnimation(data.animationName, 0.05f);
        
        // Khóa hướng quay mặt khi đang chém (giống HI3)
        // Bạn có thể thêm logic xoay nhân vật về phía kẻ địch gần nhất ở đây
    }

    protected override void UpdateState()
    {
       var data = _normalAttackCombo.attacks[_currentIndex];
        var stateInfo = _ctx.Animator.GetCurrentAnimatorStateInfo(0);
        float nTime = stateInfo.normalizedTime % 1f;

        // Cập nhật Buffer Timer
        if (_bufferTimer > 0) _bufferTimer -= Time.deltaTime;

        // Xử lý Input Buffering: Nếu bấm nút, reset timer
        if (Input.GetMouseButtonDown(0)) _bufferTimer = BUFFER_WINDOW;

        // Quét qua các Windows trong AttackData hiện tại
        foreach (var window in data.windows)
        {
            if (!window.IsInside(nTime)) continue;

            switch (window.actionName)
            {
                case "Hitbox":
                    if (!_hasDealtDamage) {
                        Debug.Log($"Gây sát thương đòn {_currentIndex}");
                        _hasDealtDamage = true;
                        _ctx._weaponHitbox.SetActive(true);
                    }
                    break;

                case "ComboInput":
                    // Nếu bấm chuột trong lúc này, đánh dấu là sẽ đánh đòn tiếp theo
                    if (Input.GetMouseButtonDown(0)) _comboQueued = true;
                    break;

                case "DashCancel":
                    if (Input.GetKeyDown(KeyCode.LeftShift)) SwitchState(_factory.Dash());
                    break;
            }
        }

       // Kiểm tra chuyển đòn (Dùng Buffer thay vì _comboQueued trực tiếp)
        if (nTime >= 0.7f) // Có thể bắt đầu buffer từ 70% animation
        {
            if (_bufferTimer > 0 && _currentIndex < _normalAttackCombo.attacks.Count - 1)
            {
                _currentIndex++;
                _bufferTimer = 0; // Reset sau khi đã dùng
                StartAttackStep();
            }
            else if (nTime >= 0.98f)
            {
                CheckSwitchState();
            }
        }
    }

    public override void CheckSwitchState()
    {
        if (_ctx.CharController.isGrounded)
            SwitchState(_factory.Grounded());
        else
            SwitchState(_factory.Falling());
       
    }
    override protected void ExitState()
    {
         _ctx._weaponHitbox.SetActive(false);
        _ctx.SetAttackLock(false);
        _ctx.SetRotationLock(false);
    }
}