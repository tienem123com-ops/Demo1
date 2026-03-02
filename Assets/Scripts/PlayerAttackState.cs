using UnityEngine;

public class PlayerAttackState : PlayerBaseState
{
    private SO_ComboChain _combo;
    private int _comboIndex;
    private bool _bufferedInput;
    private float _stateTimer;
    private float _animLength;
    private bool _canChain;

    public PlayerAttackState(PlayerController ctx, PlayerStateFactory factory)
        : base(ctx, factory)
    {
        _combo =ctx._comboChain;
      
    }

    public override void EnterState()
    {
        _ctx.SetAttackLock(true);

        _comboIndex = 0;
        PlayCurrentAttack();
    }

    protected override void UpdateState()
    {
        _stateTimer += Time.deltaTime;

        var stateInfo = _ctx.animator.GetCurrentAnimatorStateInfo(0);
        float normalized = stateInfo.normalizedTime;

        var step = _combo.comboSteps[_comboIndex];

        // Buffer input
        if (_ctx.InputHandler.IsAttacking)
            _bufferedInput = true;

        // Cho phép chain trong window
        _canChain = normalized >= step.comboWindowStart &&
                    normalized <= step.comboWindowEnd;

        // Khi animation kết thúc
        if (normalized >= 1f)
        {
            if (_bufferedInput && _comboIndex < _combo.comboSteps.Count - 1)
            {
                _comboIndex++;
                PlayCurrentAttack();
                _bufferedInput = false;
            }
            else
            {
                ExitAttack();
            }
        }
    }

    private void PlayCurrentAttack()
    {
        var step = _combo.comboSteps[_comboIndex];

        _ctx.animator.applyRootMotion = step.useRootMotion;
        _ctx.PlayAnimation(step.animationName);

        _stateTimer = 0f;
        _animLength = _ctx.animator.GetCurrentAnimatorStateInfo(0).length;
    }

    private void ExitAttack()
    {
        _ctx.animator.applyRootMotion = false;
        _ctx.SetAttackLock(false);
        _ctx.SwitchState(_factory.Grounded());
    }

    protected override void ExitState()
    {
        _bufferedInput = false;
    }
 
private void HandleAttackEnd()
{
    _ctx.OnAttackEnd -= HandleAttackEnd;
    SwitchState(_factory.Grounded());
}
}
