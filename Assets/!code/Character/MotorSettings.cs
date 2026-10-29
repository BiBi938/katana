using UnityEngine;

// CharacterMotor가 쓰는 이동 수치를 담아두는 데이터 에셋(ScriptableObject).
// CharacterMotor는 이 에셋을 "참조"로 들고 있다가 쓸 때마다 값을 읽으므로,
// 플레이 중에 에셋 값을 바꾸면 바로 반영되고, 플레이를 꺼도 바꾼 값이 에셋에 그대로 남음.
// Player, Enemy마다 다른 에셋을 만들어 연결하면 같은 CharacterMotor를 그대로 재사용할 수 있음.
// 기본값은 기존 Player 인스펙터에서 튜닝해둔 값으로, Create 메뉴로 만들 때 이 값이 채워짐.
[CreateAssetMenu(menuName = "Character/Motor Settings", fileName = "MotorSettings")]
public class MotorSettings : ScriptableObject
{
    public float speed = 7f;
    public float acceleration = 80f;     // 지상에서 목표 속도로 다가가는 빠르기
    public float airAcceleration = 30f;  // 공중에서는 더 느리게 다가가게 해서 점프 관성이 유지되게 함
    public float ctrlSpeedMultiplier = 0.3f; // Ctrl을 누르고 있을 때 x방향 이동 속도 배율

    public float jumpForce = 11f;
    public int maxJumpCount = 2;
    public float doubleJumpWindow = 1f; // 이전 점프 후 다음 점프를 쓸 수 있는 제한 시간(초)
    public float fallGravityMultiplier = 0.6f; // 하강 중 중력 배율 (1보다 크면 더 빨리 떨어짐)

    public float rollSpeed = 10f;
    public float rollDuration = 0.4f;

    public float attackDashSpeed = 18f;
    public float attackDashDuration = 0.12f;
}
