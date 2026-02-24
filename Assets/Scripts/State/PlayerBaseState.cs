using UnityEngine;

public abstract class PlayerBaseState
{
    protected PlayerBaseState(PlayerStateMachine _currentContext, PlayerStateFactory factory)
    {
        _ctx = _currentContext;
        _factory = factory;
    }

    protected bool IsRootState = false;
    protected readonly PlayerStateMachine _ctx;
    protected readonly PlayerStateFactory _factory;
    protected PlayerBaseState _childState { get; private set; }
    protected PlayerBaseState _parentState { get; private set; }
    public PlayerBaseState ChildState => _childState;

    public virtual void EnterState() { }
    protected virtual void UpdateState() { }
    protected virtual void ExitState() { }
    public virtual void CheckSwitchState() { }
    public virtual void InitializeSubState() { }

    public void UpdateStates()
    {
        UpdateState();
        _childState?.UpdateStates();
    }

    public void SwitchState(PlayerBaseState newState)
    {
        ExitState();
        newState.EnterState();

        if (IsRootState)
        {
            _ctx.CurrentState = newState;
        }
        else
        {
            _parentState?.SetChildState(newState);
        }
    }

    protected void SetChildState(PlayerBaseState _newChildState)
    {
        _childState = _newChildState;
        _childState.SetParentState(this);
    }

    protected void SetParentState(PlayerBaseState _newParentState)
    {
        _parentState = _newParentState;
    }
}      