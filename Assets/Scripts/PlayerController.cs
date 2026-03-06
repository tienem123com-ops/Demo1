using System;
using UnityEngine;
using UnityEngine.Events;


[RequireComponent(typeof(CharacterController))]
public class PlayerController : Damageable
{
    // ================= CONFIG =================

    [Header("GDC 2016 Constants")]
    [field: SerializeField] public float JumpHeight { get; private set; } = 4f;
    [field: SerializeField] public float TimeToJumpApex { get; private set; } = 0.4f;
    [field: SerializeField] public float RunMaxSpeed { get; private set; } = 12f;
    [field: SerializeField] public float RunAcceleration { get; private set; } = 90f;
    [field: SerializeField] public float Friction { get; private set; } = 30f;

    [Header("Variable Gravity")]
    [field: SerializeField] public float GravityScaling { get; private set; } = 2.5f;
    [field: SerializeField] public float FallClamp { get; private set; } = -30f;

    [Header("Coyote & Jump Buffer")]
    [field: SerializeField] public float CoyoteTime { get; private set; } = 0.15f;
    [field: SerializeField] public float JumpBufferTime { get; private set; } = 0.1f;

    [Header("Dash Settings")]
    [field: SerializeField] public float DashForce { get; private set; } = 35f;
    [field: SerializeField] public float DashCooldown { get; private set; } = 1f;
    [field: SerializeField] public float DashLength { get; private set; } = 7f;
    [field: SerializeField] public float DashDuration { get; private set; } = 0.25f;
    [field: SerializeField]
    public AnimationCurve DashCurve { get; private set; } =
        AnimationCurve.Linear(0, 1, 1, 0);

    [Header("Movement & Rotation")]
    [field: SerializeField] public float RotationSpeed { get; private set; } = 15f;
    [field: SerializeField] public float AirControl { get; private set; } = 5f;
    [field: SerializeField] public Transform Model { get; private set; }
    [field: SerializeField] public Transform MainCamera { get; private set; }
    [field: SerializeField] public Animator Animator { get; private set; }

    [Header("Slope Detection")]
    [field: SerializeField] public float MaxSlopeAngle { get; private set; } = 45f;
    [field: SerializeField] public float SlopeCheckDistance { get; private set; } = 0.5f;

    [Header("Responsive Movement (Better Movement)")]
    [field: SerializeField] public float TAttack { get; private set; } = 0.1f;
    [field: SerializeField] public float TRelease { get; private set; } = 0.15f;

    [SerializeField] private GameObject _weaponHitbox;

    // ================= SERVICES =================

    private IPhysicsHandler _physicsHandler;
    private IRotationHandler _rotationHandler;
    private IAnimationHandler _animationHandler;
    private IInputHandler _inputHandler;
    private IMovementHandler _groundMovementHandler;
    private IMovementHandler _airMovementHandler;
    private ResponsiveDecelerationHandler _decelerationHandler;

    // ================= STATE =================

    private PlayerStateFactory _states;
    private PlayerBaseState _currentState;
    private CharacterController _charController;

    // ================= RUNTIME =================

    private float _gravity;
    private float _initialJumpVelocity;

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
    private bool _attackLocked;

    // ================= PROPERTIES =================

    public CharacterController CharController => _charController;
    public PlayerBaseState CurrentState { get => _currentState; set => _currentState = value; }

    public Vector3 Velocity { get => _velocity; set => _velocity = value; }
    public Vector2 InputVector => _inputVector;

    public float CoyoteCounter
    {
        get => _coyoteCounter;
        set => _coyoteCounter = Mathf.Max(0f, value);
    }

    public float JumpBufferCounter
    {
        get => _jumpBufferCounter;
        set => _jumpBufferCounter = Mathf.Max(0f, value);
    }

    public bool IsDashing { get => _isDashing; set => _isDashing = value; }
    public bool IsAttacking => _isAttacking;

    public float Gravity => _gravity;
    public float InitialJumpVelocity => _initialJumpVelocity;

    public Vector3 GroundNormal => _lastGroundNormal;
    public bool IsOnSlope => _isOnSlope;
    public float CurrentSlopeAngle => _currentSlopeAngle;



    public IPhysicsHandler PhysicsHandler => _physicsHandler;
    public IRotationHandler RotationHandler => _rotationHandler;
    public IAnimationHandler AnimationHandler => _animationHandler;
    public IInputHandler InputHandler => _inputHandler;
    public IMovementHandler GroundMovementHandler => _groundMovementHandler;
    public IMovementHandler AirMovementHandler => _airMovementHandler;

    // ================= ANIMATION HASH (GIỮ NGUYÊN) =================

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

    // ================= COMBAT =================
    [Header("Combat")]
     public ComboSequence _normalAttackCombo;
    private bool _rotationLocked;

    private void Awake()
    {


 
        _charController = GetComponent<CharacterController>();

        if (MainCamera == null && Camera.main != null)
            MainCamera = Camera.main.transform;

        if (Animator == null)
            Animator = GetComponentInChildren<Animator>();

        ComputePhysicsConstants();
        InitializeServices();
        _states = new PlayerStateFactory(this);
        
       
    }





    public SO_PlayerConfiguration PlayerConfig { get; private set; }

    private void InitializeServices()
    {
        _physicsHandler = new GravityHandler(_gravity, GravityScaling, FallClamp);
        _rotationHandler = new ModelRotationHandler();
        _animationHandler = new MovementAnimationHandler(
            Animator,
            Anim_Idle, Anim_Run_F, Anim_Run_B, Anim_Run_L, Anim_Run_R,
            Anim_Run_FL, Anim_Run_FR, Anim_Run_BL, Anim_Run_BR);

        _inputHandler = new CameraRelativeInputHandler(MainCamera);

        _groundMovementHandler =
            new ResponsiveMovementHandler(RunMaxSpeed, TAttack, TRelease);

        _airMovementHandler =
            new ResponsiveMovementHandler(RunMaxSpeed, TAttack * 1.5f, TRelease);

        _decelerationHandler =
            new ResponsiveDecelerationHandler(RunMaxSpeed, TRelease);
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
        HandleRotation();

        _currentState.UpdateStates();
        ApplyMovement();

        _wasGroundedLastFrame = _charController.isGrounded;
    }

    private void HandleRotation()
    {
        if (_currentState is PlayerDashState || _rotationLocked) return;

        Vector3 moveDir = GetLookDirection();

        if (_isAttacking)
            _rotationHandler.RotateTowardCamera(Model, MainCamera, RotationSpeed);
        else if (_inputVector.sqrMagnitude > 0.01f)
            _rotationHandler.RotateTowardDirection(Model, moveDir, RotationSpeed);
    }

    private void ApplyMovement()
    {
        _physicsHandler.ApplyGravity(ref _velocity, _isDashing);
        if (_charController.isGrounded)
            _physicsHandler.ApplyGroundSnap(ref _velocity);
        if (!_attackLocked)
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
            if (Physics.Raycast(rayStart, Vector3.down, out hit, SlopeCheckDistance))
            {
                _lastGroundNormal = hit.normal;
                _currentSlopeAngle = Vector3.Angle(_lastGroundNormal, Vector3.up);
                _isOnSlope = _currentSlopeAngle > 0.1f && _currentSlopeAngle < MaxSlopeAngle;
            }
        }
        else
        {
            _isOnSlope = false;
        }
    }

    private void ComputePhysicsConstants()
    {
        _gravity = -(2f * JumpHeight) / Mathf.Pow(TimeToJumpApex, 2f);
        _initialJumpVelocity = Mathf.Abs(_gravity) * TimeToJumpApex;
    }

    private void UpdateTimers()
    {
        _jumpBufferCounter = Input.GetButtonDown("Jump")
            ? JumpBufferTime
            : Mathf.Max(0f, _jumpBufferCounter - Time.deltaTime);

        _coyoteCounter = _charController.isGrounded
            ? CoyoteTime
            : (_wasGroundedLastFrame ? CoyoteTime : Mathf.Max(0f, _coyoteCounter - Time.deltaTime));
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
        return moveDir.sqrMagnitude < 0.01f ? (Model?.forward ?? Vector3.forward) : moveDir;
    }
    public void SwitchState(PlayerBaseState newState)
    {
        _currentState.SwitchState(newState);

    }

    public void SetAttackLock(bool value)
    {
        _attackLocked = value;
    }
     public void SetRotationLock(bool v)
    {
        _rotationLocked = v;
    }
    private void OnAnimatorMove()
    {
        if (_attackLocked && Animator.applyRootMotion)
        {
            Vector3 delta = Animator.deltaPosition;
            _charController.Move(delta);
        }
    }


    public bool CanDash() => _dashCooldownTimer <= 0f;
    public void ResetDashCooldown() => _dashCooldownTimer = DashCooldown;

    public void SetVelocity(float x, float y, float z) => _velocity = new Vector3(x, y, z);
    public void SetVelocityX(float x) => _velocity.x = x;
    public void SetVelocityY(float y) => _velocity.y = y;
    public void SetVelocityZ(float z) => _velocity.z = z;
    public void AddVelocity(Vector3 delta) => _velocity += delta;

   
}

