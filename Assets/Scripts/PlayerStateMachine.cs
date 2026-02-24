using UnityEngine;
using StatMaster; // Đảm bảo đã cài đặt package
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
    public float dashCooldown = 1f;
    private float _dashCooldownTimer;
    public float dashLength = 7.0f;    // Quãng đường lướt (mét)
    public float dashDuration = 0.25f; // Thời gian lướt (giây)
    public AnimationCurve dashCurve = AnimationCurve.Linear(0, 1, 1, 0); // Giảm dần
    public bool IsDashing;

    [Header("Movement & Rotation")]
    public float rotationSpeed = 15f;
    public float airControl = 5f; // Thêm lại biến này để sửa lỗi FallingState
    public Transform model;       // Mesh nhân vật để xoay
    public Transform mainCamera;

   

    [HideInInspector] public float gravity;
    [HideInInspector] public float initialJumpVelocity;
    public CharacterController CharController { get; private set; }
    public PlayerBaseState CurrentState { get; set; }
    public bool IsAttacking { get; private set; }


    private PlayerStateFactory _states;
    public Vector3 Velocity;
    public Vector2 InputVector;
    public float CoyoteCounter;
    public float JumpBufferCounter;
    public Animator animator;

    // Khai báo trong vùng Variables của PlayerController
    public readonly int IDVertical = Animator.StringToHash("Vertical");
    public readonly int IDHorizontal = Animator.StringToHash("Horizontal");
    public readonly int IDSpeed = Animator.StringToHash("Speed");
    public readonly int IDJump = Animator.StringToHash("Jump");
    public readonly int IDFall = Animator.StringToHash("Fall");
    public readonly int IDDash = Animator.StringToHash("Dash");
    // DI CHUYỂN 8 HƯỚNG
    public readonly int Anim_Idle = Animator.StringToHash("HumanM@Idle01");
    public readonly int Anim_Run_F = Animator.StringToHash("HumanM@Run01_Forward");
    public readonly int Anim_Run_B = Animator.StringToHash("HumanM@Run01_Backward");
    public readonly int Anim_Run_L = Animator.StringToHash("HumanM@Run01_Left");
    public readonly int Anim_Run_R = Animator.StringToHash("HumanM@Run01_Right");
    public readonly int Anim_Run_FL = Animator.StringToHash("HumanM@Run01_ForwardLeft");
    public readonly int Anim_Run_FR = Animator.StringToHash("HumanM@Run01_ForwardRight");
    public readonly int Anim_Run_BL = Animator.StringToHash("HumanM@Run01_BackwardLeft");
    public readonly int Anim_Run_BR = Animator.StringToHash("HumanM@Run01_BackwardRight");

    // CÁC ANIMATION CẦN THIẾT KHÁC
    public readonly int Anim_Jump_Begin = Animator.StringToHash("HumanM@Jump01 - Begin");
    public readonly int Anim_Falling = Animator.StringToHash("HumanM@Fall01");
    public readonly int Anim_Land = Animator.StringToHash("HumanM@Jump01 - Land");
    public readonly int Anim_Dash = Animator.StringToHash("HumanM@Dash01"); // Thường là sprint hoặc dash
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
        // 1. Cập nhật Input và Timers
        InputVector = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        HandleTimers();
        HandleRotation();
        IsAttacking = Input.GetMouseButton(0);

        // 2. Chạy logic của State (Run/Idle/Dash sẽ cập nhật biến Velocity)
        CurrentState.UpdateStates();

        // 3. THỰC THI VẬT LÝ TẬP TRUNG (Dòng duy nhất gọi Move)
        HandleUnifiedPhysics();
    }

    private void HandleUnifiedPhysics()
    {
        // Nếu không Dash, áp dụng trọng lực
        if (!IsDashing)
        {
            if (CharController.isGrounded && Velocity.y < 0)
            {
                Velocity.y = -2f;
            }
            else
            {
                float multiplier = (Velocity.y < 0) ? gravityScaling : 1f;
                Velocity.y += gravity * multiplier * Time.deltaTime;
                Velocity.y = Mathf.Max(Velocity.y, fallClamp);
            }
        }
        // Nếu đang Dash, Velocity.y đã được PlayerDashState gán bằng 0

        // Thực thi di chuyển thực tế
        CharController.Move(Velocity * Time.deltaTime);

        // Quản lý Dash Cooldown
        if (_dashCooldownTimer > 0) _dashCooldownTimer -= Time.deltaTime;
    }

    // XÓA BỎ HOÀN TOÀN HÀM ApplyPhysics() CŨ ĐỂ TRÁNH XUNG ĐỘT
    public int GetMovementAnimation()
    {
        // 1. Nếu không bấm phím nào thì đứng yên
        if (InputVector.sqrMagnitude < 0.01f) return Anim_Idle;

        float x = InputVector.x;
        float y = InputVector.y;

        // 2. KHI TẤN CÔNG (Combat Mode / Strafe Mode)
        if (IsAttacking)
        {
            // Nhóm DI CHUYỂN LÙI (y < 0)
            if (y < -0.1f)
            {
                if (x < -0.1f) return Anim_Run_BL; // Lùi - Trái
                if (x > 0.1f) return Anim_Run_BR;  // Lùi - Phải
                return Anim_Run_B;                 // Lùi thẳng
            }

            // Nhóm DI CHUYỂN TIẾN (y > 0)
            if (y > 0.1f)
            {
                if (x < -0.1f) return Anim_Run_FL; // Tiến - Trái
                if (x > 0.1f) return Anim_Run_FR;  // Tiến - Phải
                return Anim_Run_F;                 // Tiến thẳng
            }

            // Nhóm DI CHUYỂN NGANG THUẦN TÚY (y gần bằng 0)
            if (x < -0.1f) return Anim_Run_L;      // Ngang - Trái
            if (x > 0.1f) return Anim_Run_R;       // Ngang - Phải
        }

        // 3. BÌNH THƯỜNG (Free Movement)
        // Model tự xoay theo hướng phím nên luôn dùng Run_Forward
        return Anim_Run_F;
    }
    private void HandleRotation()
    {
        if (CurrentState is PlayerDashState) return;

        // Nếu ĐANG TẤN CÔNG: Luôn nhìn về hướng Camera
        if (IsAttacking)
        {
            Vector3 cameraForward = mainCamera.forward;
            cameraForward.y = 0;
            if (cameraForward != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(cameraForward);
                model.rotation = Quaternion.Slerp(model.rotation, targetRotation, rotationSpeed * 2f * Time.deltaTime);
            }
        }
        // Nếu KHÔNG TẤN CÔNG: Chỉ nhìn theo hướng phím bấm
        else if (InputVector.sqrMagnitude > 0.01f)
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

    private int _lastPlayedHash;
  

    public void PlayAnimation(int animHash, float transition = 0.1f)
    {
        if (animator != null && _lastPlayedHash != animHash)
        {
            _lastPlayedHash = animHash;
            animator.CrossFadeInFixedTime(animHash, transition);
        }
    }
    // Nếu bạn vẫn muốn dùng string để debug nhanh
    public void PlayAnimation(string animName, float transition = 0.1f)
    {
        if (animator != null)
        {
            animator.CrossFadeInFixedTime(animName, transition);
        }
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
        CoyoteCounter = CharController.isGrounded ?1f : CoyoteCounter - Time.deltaTime;
    }

    

    public bool CanDash() => _dashCooldownTimer <= 0;
    public void ResetDashCooldown() => _dashCooldownTimer = dashCooldown;
}