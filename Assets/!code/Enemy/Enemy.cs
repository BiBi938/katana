using UnityEngine;

// 적의 "두뇌" — 플레이어와의 거리를 보고 무엇을 할지(대기/추적) 판단해서 CharacterMotor에 지시만 내림.
// Player가 키보드 입력으로 motor를 조종하듯, Enemy는 AI 판단으로 같은 motor를 조종함.
// 실제로 무엇을 할지는 상태 클래스(EnemyStates.cs)가 정하고, 이 클래스는 상태들이 쓰는 기능만 제공함.
[RequireComponent(typeof(CharacterMotor))]
public class Enemy : MonoBehaviour
{
    // 판단 수치 데이터 에셋. 인스펙터에서 EnemyData 에셋(ED_Grunt 등)을 끌어다 연결함.
    [SerializeField] EnemyData data;
    public EnemyData Data => data;

    public CharacterMotor Motor { get; private set; }
    public Transform Target { get; private set; }

    // 같은 오브젝트에 붙은 공격 컴포넌트(GruntAttack 등). 부모 타입으로 찾으므로 어떤 공격이든 받을 수 있음.
    // 없으면 공격하지 않고 추적만 함.
    public EnemyAttack Attack { get; private set; }

    Animator anim;
    SpriteRenderer spriter;
    StateMachine<Enemy> sm;

    void Awake()
    {
        if (data == null)
        {
            Debug.LogError($"{name}: Enemy에 EnemyData 에셋이 연결되지 않았습니다.", this);
            enabled = false;
            return;
        }

        Motor = GetComponent<CharacterMotor>();
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        Attack = GetComponent<EnemyAttack>();
        sm = new StateMachine<Enemy>(this);
    }

    void Start()
    {
        // 맵 전환을 Core 씬 + 맵 씬 구조로 할 예정이라 Player와 적은 서로 다른 씬에 있게 됨.
        // 씬이 다르면 인스펙터로 끌어다 연결할 수 없으므로, Start에서 태그로 한 번 찾아서 캐싱함.
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) Target = player.transform;
        else Debug.LogWarning($"{name}: 'Player' 태그를 가진 오브젝트를 찾지 못해 추적하지 않습니다.", this);

        sm.ChangeState(new EnemyIdleState());
    }

    void Update()
    {
        sm.Tick(Time.deltaTime);
    }

    void LateUpdate()
    {
        if (anim != null) anim.SetFloat("speed", Mathf.Abs(Motor.MoveInputX));
    }

    public void ChangeState(IState<Enemy> next) => sm.ChangeState(next);

    // 상태 클래스에서 호출: dirX가 -1/1이면 그 방향으로 걷고, 0이면 멈춤.
    public void Move(float dirX)
    {
        Motor.SetMoveInput(new Vector2(dirX, 0f), false);
        if (dirX != 0f && spriter != null) spriter.flipX = dirX < 0f;
    }

    // 멈춘 채로 플레이어 쪽을 바라보게 함 (공격 직전에 방향을 맞출 때 사용).
    public void FaceTarget()
    {
        if (Target != null && spriter != null) spriter.flipX = TargetDeltaX < 0f;
    }

    // 플레이어까지의 가로 거리(부호 포함). 양수면 플레이어가 오른쪽에 있음.
    public float TargetDeltaX => Target.position.x - transform.position.x;

    public bool IsTargetOnSameFloor => Mathf.Abs(Target.position.y - transform.position.y) <= data.detectHeight;

    // Scene 뷰에서 이 적을 선택했을 때 감지 범위(노란색), 추적 포기 범위(빨간색), 공격 사거리(자홍색)를 사각형으로 보여줌.
    // 게임 화면에는 그려지지 않고, EnemyData 값을 바꾸면 바로 반영되어 범위를 눈으로 맞출 수 있음.
    void OnDrawGizmosSelected()
    {
        if (data == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(data.detectRange * 2f, data.detectHeight * 2f, 0f));
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position, new Vector3(data.loseRange * 2f, data.detectHeight * 2f, 0f));
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position, new Vector3(data.attackRange * 2f, data.detectHeight * 2f, 0f));
    }
}
