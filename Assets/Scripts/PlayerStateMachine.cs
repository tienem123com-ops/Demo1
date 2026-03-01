using UnityEngine;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : Damageable
{
    // Config
    [Header("GDC 2016 Constants")]
    [SerializeField] private float _jumpHeight = 4f;
    [SerializeField] private float _timeToJumpApex = 0.4f;
    [SerializeField] private float _runMaxSpeed = 12f;
    [SerializeField] private float _runAcceleration = 90f;
    [SerializeField] private float _friction = 30f;

    [Header("Variable Gravity")]
    [SerializeField] private float _gravityScaling = 2.5f;
    [SerializeField] private float _fallClamp = -30f;

    [Header("Coyote & Jump Buffer")]
    [SerializeField] private float _coyoteTime = 0.15f;
    [SerializeField] private float _jumpBufferTime = 0.1f;

    [Header("Dash Settings")]
    [SerializeField] private float _dashForce = 35f;
    [SerializeField] private float _dashCooldown = 1f;
    [SerializeField] private float _dashLength = 7.0f;
    [SerializeField] private float _dashDuration = 0.25f;
    [SerializeField] private AnimationCurve _dashCurve = AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Movement & Rotation")]
    [SerializeField] private float _rotationSpeed = 15f;
    [SerializeField] private float _airControl = 5f;
    [SerializeField] private Transform _model;
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private Animator _animator;

    [Header("Slope Detection")]
    [SerializeField] private float _maxSlopeAngle = 45f;
    [SerializeField] private float _slopeCheckDistance = 0.5f;

    [Header("Responsive Movement (Better Movement)")]
    [SerializeField] private float _tAttack = 0.1f;      // Time to reach max speed (seconds)
    [SerializeField] private float _tRelease = 0.15f;    // Time to decelerate to stop (seconds)

    // Services (Dependency Injection)
    private IPhysicsHandler _physicsHandler;
    private IRotationHandler _rotationHandler;
    private IAnimationHandler _animationHandler;
    private IInputHandler _inputHandler;
    private IMovementHandler _groundMovementHandler;
    private IMovementHandler _airMovementHandler;
    private ResponsiveDecelerationHandler _decelerationHandler;

    // State
    private PlayerStateFactory _states;
    private PlayerBaseState _currentState;
    private CharacterController _charController;

    // Runtime
    [HideInInspector][SerializeField] private float _gravity;
    [HideInInspector][SerializeField] private float _initialJumpVelocity;

    private Vector3 _velocity = Vector3.zero;
    private Vector2 _inputVector = Vector2.zero;
    private float _coyoteCounter = 0f;
    private float _jumpBufferCounter = 0f;
    private float _dashCooldownTimer = 0f;
    private bool _isDashing = false;
    private bool _isAttacking = false;
    private bool _wasGroundedLastFrame = false;
    private Vector3 _lastGroundNormal = Vector3.up;
    private bool _isOnSlope = false;
    private float _currentSlopeAngle = 0f;

    // Properties
    public CharacterController CharController => _charController;
    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }
    public Vector3 Velocity { get => _velocity; set => _velocity = value; }
    public Vector2 InputVector => _inputVector;
    public float CoyoteCounter { get => _coyoteCounter; set => _coyoteCounter = Mathf.Max(0f, value); }
    public float JumpBufferCounter { get => _jumpBufferCounter; set => _jumpBufferCounter = Mathf.Max(0f, value); }
    public bool IsDashing { get => _isDashing; set => _isDashing = value; }
    public bool IsAttacking => _isAttacking;
    public float gravity => _gravity;
    public float initialJumpVelocity => _initialJumpVelocity;

    public float jumpHeight => _jumpHeight;
    public float timeToJumpApex => _timeToJumpApex;
    public float runMaxSpeed => _runMaxSpeed;
    public float runAcceleration => _runAcceleration;
    public float friction => _friction;
    public float gravityScaling => _gravityScaling;
    public float fallClamp => _fallClamp;
    public float dashForce => _dashForce;
    public float dashCooldown => _dashCooldown;
    public float dashLength => _dashLength;
    public float dashDuration => _dashDuration;
    public AnimationCurve dashCurve => _dashCurve;
    public float rotationSpeed => _rotationSpeed;
    public float airControl => _airControl;
    public Transform model => _model;
    public Transform mainCamera => _mainCamera;
    public Animator animator => _animator;
    public Vector3 GroundNormal => _lastGroundNormal;
    public bool IsOnSlope => _isOnSlope;
    public float CurrentSlopeAngle => _currentSlopeAngle;

    // Service accessors
    public IPhysicsHandler PhysicsHandler => _physicsHandler;
    public IRotationHandler RotationHandler => _rotationHandler;
    public IAnimationHandler AnimationHandler => _animationHandler;
    public IInputHandler InputHandler => _inputHandler;
    public IMovementHandler GroundMovementHandler => _groundMovementHandler;
    public IMovementHandler AirMovementHandler => _airMovementHandler;

    // Animator hashes
    public readonly int IDVertical = Animator.StringToHash("Vertical");
    public readonly int IDHorizontal = Animator.StringToHash("Horizontal");
    public readonly int IDSpeed = Animator.StringToHash("Speed");
    public readonly int IDJump = Animator.StringToHash("Jump");
    public readonly int IDFall = Animator.StringToHash("Fall");
    public readonly int IDDash = Animator.StringToHash("Dash");
    public readonly int Anim_Idle = Animator.StringToHash("HumanM@Idle01");
    public readonly int Anim_Run_F = Animator.StringToHash("HumanM@Run01_Forward");
    public readonly int Anim_Run_B = Animator.StringToHash("HumanM@Run01_Backward");
    public readonly int Anim_Run_L = Animator.StringToHash("HumanM@Run01_Left");
    public readonly int Anim_Run_R = Animator.StringToHash("HumanM@Run01_Right");
    public readonly int Anim_Run_FL = Animator.StringToHash("HumanM@Run01_ForwardLeft");
    public readonly int Anim_Run_FR = Animator.StringToHash("HumanM@Run01_ForwardRight");
    public readonly int Anim_Run_BL = Animator.StringToHash("HumanM@Run01_BackwardLeft");
    public readonly int Anim_Run_BR = Animator.StringToHash("HumanM@Run01_BackwardRight");
    public readonly int Anim_Jump_Begin = Animator.StringToHash("HumanM@Jump01 - Begin");
    public readonly int Anim_Falling = Animator.StringToHash("HumanM@Fall01");
    public readonly int Anim_Land = Animator.StringToHash("HumanM@Jump01 - Land");
    public readonly int Anim_Dash = Animator.StringToHash("HumanM@Dash01");

    public SO_PlayerConfiguration PlayerConfig { get; private set; }
    private void Awake()
    {
        _charController = GetComponent<CharacterController>();
        if (_mainCamera == null)
            _mainCamera = Camera.main != null ? Camera.main.transform : null;
        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        ComputePhysicsConstants();
        InitializeServices();
        _states = new PlayerStateFactory(this);
    }

    private void InitializeServices()
    {
        _physicsHandler = new GravityHandler(_gravity, _gravityScaling, _fallClamp);
        _rotationHandler = new ModelRotationHandler();
        _animationHandler = new MovementAnimationHandler(_animator,
            Anim_Idle, Anim_Run_F, Anim_Run_B, Anim_Run_L, Anim_Run_R,
            Anim_Run_FL, Anim_Run_FR, Anim_Run_BL, Anim_Run_BR);
        _inputHandler = new CameraRelativeInputHandler(_mainCamera);
        
        // Use responsive movement handlers (Better Movement pattern)
        _groundMovementHandler = new ResponsiveMovementHandler(_runMaxSpeed, _tAttack, _tRelease);
        _airMovementHandler = new ResponsiveMovementHandler(_runMaxSpeed, _tAttack * 1.5f, _tRelease);
        _decelerationHandler = new ResponsiveDecelerationHandler(_runMaxSpeed, _tRelease);
    }

    private void Start()
    {
        _currentState = _states.Grounded();
        _currentState.EnterState();
        _wasGroundedLastFrame = _charController.isGrounded;
    }

    private void Update()
    {
        _inputVector = _inputHandler.ReadMovementInput();
        _isAttacking = _inputHandler.IsAttacking;

        UpdateTimers();
        HandleRotationAndPhysics();

        _currentState.UpdateStates();
        ApplyMovement();

        _wasGroundedLastFrame = _charController.isGrounded;
    }

    private void HandleRotationAndPhysics()
    {
        if (_currentState is PlayerDashState) return;

        Vector3 moveDir = GetLookDirection();
        
        if (_isAttacking)
            _rotationHandler.RotateTowardCamera(_model, _mainCamera, _rotationSpeed);
        else if (_inputVector.sqrMagnitude > 0.01f)
            _rotationHandler.RotateTowardDirection(_model, moveDir, _rotationSpeed);
    }

    private void ApplyMovement()
    {
        _physicsHandler.ApplyGravity(ref _velocity, _isDashing);
        if (_charController.isGrounded)
            _physicsHandler.ApplyGroundSnap(ref _velocity);

        _charController.Move(_velocity * Time.deltaTime);
        DetectGroundNormal();

        if (_dashCooldownTimer > 0f)
            _dashCooldownTimer -= Time.deltaTime;
    }

    private void DetectGroundNormal()
    {
        if (_charController.isGrounded)
        {
            RaycastHit hit;
            Vector3 rayStart = transform.position + Vector3.up * 0.1f;
            if (Physics.Raycast(rayStart, Vector3.down, out hit, _slopeCheckDistance))
            {
                _lastGroundNormal = hit.normal;
                _currentSlopeAngle = Vector3.Angle(_lastGroundNormal, Vector3.up);
                _isOnSlope = _currentSlopeAngle > 0.1f && _currentSlopeAngle < _maxSlopeAngle;
            }
        }
        else
        {
            _isOnSlope = false;
        }
    }

    private void ComputePhysicsConstants()
    {
        _gravity = -(2f * _jumpHeight) / Mathf.Pow(_timeToJumpApex, 2f);
        _initialJumpVelocity = Mathf.Abs(_gravity) * _timeToJumpApex;
    }

    private void UpdateTimers()
    {
        _jumpBufferCounter = Input.GetButtonDown("Jump") 
            ? _jumpBufferTime 
            : Mathf.Max(0f, _jumpBufferCounter - Time.deltaTime);

        _coyoteCounter = _charController.isGrounded 
            ? _coyoteTime 
            : (_wasGroundedLastFrame ? _coyoteTime : Mathf.Max(0f, _coyoteCounter - Time.deltaTime));
    }

    public Vector3 GetLookDirection() => _inputHandler.GetMovementDirection(_inputVector);

    public int GetMovementAnimation() => _animationHandler.GetMovementAnimation(_inputVector, _isAttacking);

    public void PlayAnimation(int animHash, float transition = 0.1f) 
        => _animationHandler.PlayAnimation(animHash, transition);

    public void PlayAnimation(string animName, float transition = 0.1f) 
        => _animationHandler.PlayAnimation(animName, transition);

    public Vector3 GetHorizontalDashDirection()
    {
        Vector3 moveDir = GetLookDirection();
        return moveDir.sqrMagnitude < 0.01f ? (_model?.forward ?? Vector3.forward) : moveDir;
    }

    public bool CanDash() => _dashCooldownTimer <= 0f;
    public void ResetDashCooldown() => _dashCooldownTimer = _dashCooldown;

    public void SetVelocity(float x, float y, float z) => _velocity = new Vector3(x, y, z);
    public void SetVelocityX(float x) => _velocity.x = x;
    public void SetVelocityY(float y) => _velocity.y = y;
    public void SetVelocityZ(float z) => _velocity.z = z;
    public void AddVelocity(Vector3 delta) => _velocity += delta;
}

