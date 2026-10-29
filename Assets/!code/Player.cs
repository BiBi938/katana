using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    public Vector2 inputVec;
    Animator anim;

    public float attackRange = 5f;
    public Vector2 attackTarget;

    // 이동/점프/중력/구르기/공격대시 같은 물리 처리는 CharacterMotor(+ GroundSensor)가 전담.
    // 이동 수치는 CharacterMotor에 연결된 MotorSettings 에셋에서 조절함.
    // Player는 입력을 해석해서 motor에 지시만 내리고, 애니메이터 파라미터를 갱신하는 역할만 함.
    CharacterMotor motor;

    bool isCtrl;

    // EffectDefinition: 이펙트 하나의 "설정값"을 담아두는 ScriptableObject.
    // 어떤 프레팹을 재생할지, 어느 소켓 위치에서 재생할지, 몇 초 뒤 반환할지 등을
    // 코드가 아니라 에셋(인스펙터)에서 정의해두는 데이터일 뿐이라 코드 수정 없이 값만 바꿀 수 있음.
    // attackEffect: 이번 공격 모션에서 재생할 이펙트의 정의(DEF_SlashAttack 등)를 인스펙터에서 연결해두는 용도.
    // attack.anim의 타격 프레임에 건 Animation Event(OnAttackEffect)에서 effectPlayer.Play(attackEffect, ...)로 실제 재생에 사용됨.
    public EffectDefinition attackEffect;

    // EffectPlayer: 이 GameObject(Player)에 붙어 있는 소켓들(WeaponSocket 등 Transform 위치)을 관리하면서
    // EffectDefinition을 받아 해당 소켓 위치에 이펙트 프레팹을 실제로 스폰/재생해주는 컴포넌트.
    EffectPlayer effectPlayer;

    SpriteRenderer spriter;

    public ParticleSystem dust;

    // motor.IsGrounded는 물리 접촉 기반이라 점프 직후 한두 프레임은 값이 갱신되지 않고 그대로일 수 있음.
    // 그동안 OnJump에서 강제로 false로 지정한 애니메이터 값을 이 변수가 덮어쓰지 않도록 변경(edge)이
    // 있을 때만 애니메이터를 갱신하기 위한 추적용 값.
    bool wasGroundedAnim;

    private void Awake()
    {
        spriter = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        effectPlayer = GetComponent<EffectPlayer>();
        motor = GetComponent<CharacterMotor>();
    }

    void OnMove(InputValue value)
    {
        inputVec = value.Get<Vector2>();
        motor.SetMoveInput(inputVec, isCtrl);
    }

    void OnJump(InputValue value)
    {
        if (!value.isPressed) return;
        if (!motor.RequestJump()) return;

        anim.SetBool("isGrounded", false);
        anim.SetBool("isFalling", false);
        anim.SetTrigger("Jump");
    }

    void OnRoll(InputValue value)
    {
        if (!value.isPressed) return;

        float dirX = spriter.flipX ? -1f : 1f;
        if (!motor.RequestRoll(dirX)) return;

        anim.SetBool("isRolling", true);
        anim.SetTrigger("Roll");
    }

    private void LateUpdate()// 매 프레임 Update 다음에 실행되는 생명주기 함수
    {
        //플레이어의애니메이터 를 조작하는 변수,(speed, dead를 선언해놓음.)
        //근데 speed에 벡터의 순수한 값인 크기를 넣어줌. -> 그러면 파라미터 speed가 바뀌면서 애니메이션 상태가 바뀜.
        anim.SetFloat("speed", inputVec.magnitude);

        isCtrl = Keyboard.current != null && (Keyboard.current.leftCtrlKey.isPressed || Keyboard.current.rightCtrlKey.isPressed);
        anim.SetBool("isCtrl", isCtrl);
        motor.SetMoveInput(inputVec, isCtrl); // isCtrl은 여기서 매 프레임 갱신되므로 motor에도 다시 반영

        bool grounded = motor.IsGrounded;
        if (grounded != wasGroundedAnim)
        {
            wasGroundedAnim = grounded;
            anim.SetBool("isGrounded", grounded);
        }
        anim.SetBool("isFalling", motor.IsFalling);
        anim.SetBool("isRolling", motor.IsRolling);

        //구르는 중이거나 공격 대시 중이 아니면서 x축으로 움직였을 때
        if (!motor.IsRolling && !motor.IsAttackDashing && inputVec.x != 0)
        {
            //왼쪽으로 이동하면 스프라이트 뒤집기. = flipX를 트루
            bool newFlipX = inputVec.x < 0; //ex: 왼(음수)면 true, 오(양수)면 flase
            if (newFlipX != spriter.flipX) //newfilpx가 현재 방향과 다르면
            {
                spriter.flipX = newFlipX;//
                CreateDust();
            }
        }

        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            TryAttack();
        }
    }

    void TryAttack()
    {
        if (motor.IsRolling) return;

        Vector2 mouseScreen = Mouse.current.position.ReadValue();
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(
            new Vector3(mouseScreen.x, mouseScreen.y, -Camera.main.transform.position.z));
        mouseWorld.z = 0f;

        Vector2 toMouse = (Vector2)mouseWorld - (Vector2)transform.position;
        float dist = toMouse.magnitude;
        if (dist < 0.0001f) return;

        Vector2 dir = toMouse / dist;
        attackTarget = dist <= attackRange
            ? (Vector2)mouseWorld
            : (Vector2)transform.position + dir * attackRange;

        if (Mathf.Abs(dir.x) > 0.0001f)
        {
            spriter.flipX = dir.x < 0f;
        }

        float dirX = spriter.flipX ? -1f : 1f;
        motor.RequestAttackDash(dirX);

        anim.SetTrigger("Attack");
    }

    void CreateDust(){
        dust.Play();
    }

    // attack.anim의 타격 프레임에 등록할 Animation Event용 콜백.
    void OnAttackEffect()
    {
        effectPlayer.Play(attackEffect, spriter.flipX);
    }

}
