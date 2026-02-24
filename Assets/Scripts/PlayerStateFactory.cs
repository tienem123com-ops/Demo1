public class PlayerStateFactory
{
    PlayerStateMachine _context;
    public PlayerStateFactory(PlayerStateMachine currentContext) => _context = currentContext;

    public PlayerBaseState Grounded() => new PlayerGroundedState(_context, this);
    public PlayerBaseState Jumping() => new PlayerJumpingState(_context, this);
    public PlayerBaseState Falling() => new PlayerFallingState(_context, this); 
    public PlayerBaseState Idle() => new PlayerIdleState(_context, this);
    public PlayerBaseState Dash() => new PlayerDashState(_context, this);
    public PlayerBaseState Run() => new PlayerRunState(_context, this);
}