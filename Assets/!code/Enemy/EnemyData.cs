using UnityEngine;

// 적 한 종류의 "판단" 수치를 담아두는 데이터 에셋(ScriptableObject).
// 이동 속도·점프 같은 몸의 수치는 MotorSettings, 언제 쫓아가고 멈출지 같은 두뇌의 수치는 이 에셋에서 조절함.
// Grunt, Pomp마다 에셋을 하나씩 만들어(ED_Grunt, ED_Pomp) Enemy 컴포넌트에 연결하면
// 같은 Enemy 코드로 적마다 다르게 행동하게 만들 수 있음.
[CreateAssetMenu(menuName = "Character/Enemy Data", fileName = "EnemyData")]
public class EnemyData : ScriptableObject
{
    public float detectRange = 6f;  // 가로 거리가 이 안으로 들어오면 추적 시작
    public float detectHeight = 1.5f; // 위아래 높이 차이가 이보다 크면 다른 층으로 보고 감지하지 않음
    public float loseRange = 9f;    // 추적 중 가로 거리가 이보다 멀어지면 포기. detectRange보다 크게 해야 경계에서 대기/추적이 계속 바뀌지 않음
    public float stopDistance = 1f; // 가로 거리가 이만큼 가까워지면 멈춤

    public float attackRange = 1.2f;    // 가로 거리가 이 안이고 쿨타임이 끝났으면 공격 시작
    public float attackCooldown = 1.5f; // 공격이 끝난 뒤 다음 공격까지 기다리는 시간(초)
}
