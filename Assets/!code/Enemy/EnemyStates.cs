using UnityEngine;

// 대기 상태: 멈춰 있다가 플레이어가 같은 층의 감지 거리 안에 들어오면 추적 상태로 바뀜.
public class EnemyIdleState : IState<Enemy>
{
    public void Enter(Enemy enemy)
    {
        enemy.Move(0f);
    }

    public void Exit(Enemy enemy) { }

    public void Tick(Enemy enemy, float dt)
    {
        if (enemy.Target == null) return;

        if (enemy.IsTargetOnSameFloor && Mathf.Abs(enemy.TargetDeltaX) <= enemy.Data.detectRange)
        {
            enemy.ChangeState(new EnemyChaseState());
        }
    }
}

// 추적 상태: 플레이어 쪽으로 걸어가다가 공격 사거리 안이고 공격할 수 있으면 공격 상태로 바뀜.
// 쿨타임 중이면 stopDistance까지 다가가서 기다리고, 다른 층으로 가거나 loseRange보다 멀어지면 포기하고 대기 상태로 돌아감.
public class EnemyChaseState : IState<Enemy>
{
    public void Enter(Enemy enemy) { }

    public void Exit(Enemy enemy)
    {
        enemy.Move(0f);
    }

    public void Tick(Enemy enemy, float dt)
    {
        if (enemy.Target == null || !enemy.IsTargetOnSameFloor || Mathf.Abs(enemy.TargetDeltaX) > enemy.Data.loseRange)
        {
            enemy.ChangeState(new EnemyIdleState());
            return;
        }

        float deltaX = enemy.TargetDeltaX;
        if (enemy.Attack != null && enemy.Attack.IsReady && Mathf.Abs(deltaX) <= enemy.Data.attackRange)
        {
            enemy.ChangeState(new EnemyAttackState());
            return;
        }

        enemy.Move(Mathf.Abs(deltaX) <= enemy.Data.stopDistance ? 0f : Mathf.Sign(deltaX));
    }
}

// 공격 상태: 멈춰서 플레이어 쪽을 보고 공격을 시작한 뒤, 공격이 끝나면 추적 상태로 돌아감.
// 어떻게 공격하는지는 모르고 EnemyAttack에 시키기만 하므로, 적 종류가 늘어나도 이 상태는 그대로 씀.
public class EnemyAttackState : IState<Enemy>
{
    public void Enter(Enemy enemy)
    {
        enemy.Move(0f);
        enemy.FaceTarget();
        enemy.Attack.Begin(enemy);
    }

    public void Exit(Enemy enemy) { }

    public void Tick(Enemy enemy, float dt)
    {
        if (!enemy.Attack.IsAttacking) enemy.ChangeState(new EnemyChaseState());
    }
}
