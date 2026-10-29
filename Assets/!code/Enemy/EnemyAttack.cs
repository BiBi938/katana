using UnityEngine;

// 모든 적 공격 컴포넌트의 공통 부모. Enemy(두뇌)는 이 타입만 알고 "공격해"라고 시키기만 하고,
// 실제로 어떻게 공격하는지(베기/발차기 등)는 GruntAttack, PompAttack 같은 자식 클래스가 OnBegin에서 정함.
// 공격 중 여부, 쿨타임, 공격이 끝났을 때의 처리는 모든 적이 같으므로 여기서 처리함.
// abstract라서 이 컴포넌트 자체는 Add Component로 붙일 수 없고, 자식 클래스(GruntAttack 등)를 붙여야 함.
public abstract class EnemyAttack : MonoBehaviour
{
    // 공격 애니메이션 끝의 OnAttackEnd 이벤트를 빠뜨렸을 때 공격 상태에 갇히지 않게 하는 안전장치(초).
    // 게임 수치가 아니라 실수 방지용 값이라 데이터 에셋으로 빼지 않음.
    const float SafetyDuration = 3f;

    public bool IsAttacking { get; private set; }
    public bool IsReady => !IsAttacking && cooldownTimer <= 0f;

    float cooldown;
    float cooldownTimer;
    float safetyTimer;

    // EnemyAttackState가 공격을 시작할 때 호출.
    public void Begin(Enemy enemy)
    {
        IsAttacking = true;
        cooldown = enemy.Data.attackCooldown;
        safetyTimer = SafetyDuration;
        OnBegin(enemy);
    }

    // 자식 클래스가 실제 공격 동작(애니메이션 트리거 등)을 시작하는 곳.
    protected abstract void OnBegin(Enemy enemy);

    // 공격 애니메이션의 마지막 프레임에 등록할 Animation Event용 콜백.
    // 이 시점부터 쿨타임을 세기 시작함.
    public void OnAttackEnd()
    {
        if (!IsAttacking) return;
        IsAttacking = false;
        cooldownTimer = cooldown;
    }

    protected virtual void Update()
    {
        if (IsAttacking)
        {
            safetyTimer -= Time.deltaTime;
            if (safetyTimer <= 0f)
            {
                Debug.LogWarning($"{name}: 공격 애니메이션에 OnAttackEnd 이벤트가 없어서 안전장치로 공격을 끝냈습니다.", this);
                OnAttackEnd();
            }
        }
        else if (cooldownTimer > 0f)
        {
            cooldownTimer -= Time.deltaTime;
        }
    }
}
