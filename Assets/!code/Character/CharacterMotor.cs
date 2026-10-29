using UnityEngine;

// 캐릭터의 "몸" — 속도/중력/점프/구르기/공격대시 같은 물리 이동을 전담.
// 입력이 무엇인지(키보드/마우스 vs AI)는 전혀 모르고, 밖에서 SetMoveInput/RequestJump/
// RequestRoll/RequestAttackDash로 지시받은 대로만 물리를 처리함. GroundSensor와 함께
// Player, Enemy 등 물리 기반으로 걷는 캐릭터라면 그대로 재사용 가능.
[RequireComponent(typeof(Rigidbody2D), typeof(GroundSensor))]
public class CharacterMotor : MonoBehaviour
{
    // 이동 수치 데이터 에셋. 인스펙터에서 MotorSettings 에셋을 끌어다 연결함.
    // 값을 복사해두지 않고 에셋을 참조하므로 플레이 중에 에셋 값을 바꾸면 바로 반영됨.
    [SerializeField] MotorSettings settings;
    public MotorSettings Settings => settings;

    public bool IsGrounded => groundSensor.IsGrounded;
    public bool IsFalling => !IsGrounded && rigid.velocity.y < -0.01f && jumpCount >= Settings.maxJumpCount;
    public bool IsRolling => dashKind == DashState.Kind.Roll;
    public bool IsAttackDashing => dashKind == DashState.Kind.AttackDash;

    // MoveState가 읽는 입력/속도 상태. Player/EnemyAI 등 드라이버가 SetMoveInput으로 갱신함.
    public float MoveInputX { get; private set; }
    public bool SlowedInput { get; private set; }
    public float VelocityX { get; private set; }

    Rigidbody2D rigid;
    GroundSensor groundSensor;
    StateMachine<CharacterMotor> sm;

    int jumpCount;
    float jumpWindowTimer;
    float baseGravityScale;
    bool wasGrounded;
    DashState.Kind? dashKind;

    void Awake()
    {
        // 인스펙터에서 에셋 연결을 빠뜨리면 FixedUpdate에서 매 프레임 NullReferenceException이 나므로 여기서 미리 알려줌
        if (settings == null)
        {
            Debug.LogError($"{name}: CharacterMotor에 MotorSettings 에셋이 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        rigid = GetComponent<Rigidbody2D>();
        groundSensor = GetComponent<GroundSensor>();
        baseGravityScale = rigid.gravityScale;

        sm = new StateMachine<CharacterMotor>(this);
        sm.ChangeState(new MoveState());
    }

    void FixedUpdate()
    {
        sm.Tick(Time.fixedDeltaTime);

        rigid.gravityScale = rigid.velocity.y < 0f ? baseGravityScale * Settings.fallGravityMultiplier : baseGravityScale;

        UpdateGroundedState();

        if (jumpWindowTimer > 0f)
        {
            jumpWindowTimer -= Time.fixedDeltaTime;
            if (jumpWindowTimer <= 0f)
            {
                jumpWindowTimer = 0f;
                jumpCount = Settings.maxJumpCount; // 제한 시간 안에 다음 점프를 안 쓰면 기회가 사라짐
            }
        }
    }

    void UpdateGroundedState()
    {
        bool isGrounded = IsGrounded;
        if (isGrounded == wasGrounded) return;

        wasGrounded = isGrounded;
        if (isGrounded)
        {
            jumpCount = 0;
            jumpWindowTimer = 0f;
        }
    }

    public void SetMoveInput(Vector2 dir, bool slowed)
    {
        MoveInputX = dir.x;
        SlowedInput = slowed;
    }

    public bool RequestJump()
    {
        if (jumpCount >= Settings.maxJumpCount) return false;
        if (jumpCount > 0 && jumpWindowTimer <= 0f) return false; // 제한 시간이 끝났으면 추가 점프 불가

        jumpCount++;
        rigid.velocity = new Vector2(rigid.velocity.x, Settings.jumpForce);
        jumpWindowTimer = jumpCount < Settings.maxJumpCount ? Settings.doubleJumpWindow : 0f;
        return true;
    }

    public bool RequestRoll(float dirX)
    {
        if (IsRolling) return false;
        sm.ChangeState(new DashState(DashState.Kind.Roll, dirX, Settings.rollSpeed, Settings.rollDuration));
        return true;
    }

    public bool RequestAttackDash(float dirX)
    {
        if (IsRolling) return false; // 구르는 중엔 공격 불가
        sm.ChangeState(new DashState(DashState.Kind.AttackDash, dirX, Settings.attackDashSpeed, Settings.attackDashDuration));
        return true;
    }

    // 상태 클래스(MoveState/DashState)에서만 호출하는 내부 API.
    public void ApplyHorizontalVelocity(float velocityX)
    {
        VelocityX = velocityX;
        rigid.velocity = new Vector2(velocityX, rigid.velocity.y);
    }

    public void ChangeState(IState<CharacterMotor> next) => sm.ChangeState(next);

    public void SetDashKind(DashState.Kind? kind) => dashKind = kind;
}
