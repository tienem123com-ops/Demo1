using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerStateMachine : MonoBehaviour
{
    [Header("GDC 2016 Constants")]
    public float jumpHeight = 4f;
    public float timeToJumpApex = 0.4f;
    public float runMaxSpeed = 12f;
    public float runAcceleration = 90f;
    public float friction = 30f;

    [Header("Variable Gravity")]
    public float gravityScaling = 2.5f;
    public float fallClamp = -30f;

    [Header("Dash Settings")]
    public float dashForce = 35f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private float _dashCooldownTimer;

    [Header("Movement & Rotation")]
    public float rotationSpeed = 15f;
    public float airControl = 5f; // Thêm lại biến này để sửa lỗi FallingState
    public Transform model;       // Mesh nhân vật để xoay
    public Transform mainCamera;

    [HideInInspector] public float gravity;
    [HideInInspector] public float initialJumpVelocity;
    public CharacterController CharController { get; private set; }
    public PlayerBaseState CurrentState { get; set; }
    private PlayerStateFactory _states;
    public Vector3 Velocity;
    public Vector2 InputVector;
    public float CoyoteCounter;
    public float JumpBufferCounter;
    public Animator animator;

    void Awake()
    {
        CharController = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;

        // Tính toán vật lý Pittman
        gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        initialJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

        _states = new PlayerStateFactory(this);
    }

    void Start()
    {
        CurrentState = _states.Grounded();
        CurrentState.EnterState();
    }

    void Update()
    {
        InputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        HandleTimers();
        HandleRotation(); // Logic xoay model 360 độ

        CurrentState.UpdateStates();
        ApplyPhysics();

        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
    }

    private void HandleRotation()
    {
        // Xoay model theo hướng di chuyển phím bấm + Camera
        if (InputVector.sqrMagnitude > 0.01f && !(CurrentState is PlayerDashState))
        {
            Vector3 moveDir = GetLookDirection();
            if (moveDir != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDir);
                model.rotation = Quaternion.Slerp(model.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
    }

    public Vector3 GetLookDirection()
    {
        Vector3 f = mainCamera.forward;
        Vector3 r = mainCamera.right;
        f.y = 0; r.y = 0;
        f.Normalize();
        r.Normalize();
        return (f * InputVector.y + r * InputVector.x).normalized;
    }

    // Sửa lỗi thiếu hàm GetHorizontalDashDirection
    public Vector3 GetHorizontalDashDirection()
    {
        Vector3 moveDir = GetLookDirection();
        if (moveDir.sqrMagnitude < 0.01f) return model.forward;
        return moveDir;
    }

    private void ApplyPhysics()
    {
        if (CurrentState is PlayerDashState) return;

        if (!CharController.isGrounded)
        {
            float multiplier = (Velocity.y < 0) ? gravityScaling : 1f;
            Velocity.y += gravity * multiplier * Time.deltaTime;
            Velocity.y = Mathf.Max(Velocity.y, fallClamp);
        }
        else if (Velocity.y < 0) Velocity.y = -2f;

        CharController.Move(Velocity * Time.deltaTime);
    }

    private void HandleTimers()
    {
        if (Input.GetButtonDown("Jump")) JumpBufferCounter = 0.2f;
        else JumpBufferCounter -= Time.deltaTime;
        CoyoteCounter = CharController.isGrounded ? 0.15f : CoyoteCounter - Time.deltaTime;
    }

    public void PlayAnimation(string animName, float transition = 0.1f)
    {
        if (animator != null) animator.CrossFadeInFixedTime(animName, transition);
    }

    public bool CanDash() => _dashCooldownTimer <= 0;
    public void ResetDashCooldown() => _dashCooldownTimer = dashCooldown;
}