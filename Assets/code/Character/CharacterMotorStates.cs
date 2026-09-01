using UnityEngine;

// 평소 이동 상태: 입력 방향으로 목표 속도를 정하고 가속도만큼씩 다가가서 관성을 만듦.
// 지상/공중에 따라 다른 가속도를 써서 공중에서는 방향 전환이 느리게(점프 관성 유지) 됨.
public class MoveState : IState<CharacterMotor>
{
    public void Enter(CharacterMotor motor) { }
    public void Exit(CharacterMotor motor) { }

    public void Tick(CharacterMotor motor, float dt)
    {
        float targetVelocityX = motor.MoveInputX * motor.Config.speed * (motor.SlowedInput ? motor.Config.ctrlSpeedMultiplier : 1f);
        float accel = motor.IsGrounded ? motor.Config.acceleration : motor.Config.airAcceleration;
        float newVelocityX = Mathf.MoveTowards(motor.VelocityX, targetVelocityX, accel * dt);
        motor.ApplyHorizontalVelocity(newVelocityX);
    }
}

// 구르기/공격대시 공용 상태: 정해진 시간 동안 고정된 수평 속도로 이동하다가 끝나면 MoveState로 복귀.
// kind로 구르기/공격대시를 구분해서 CharacterMotor.IsRolling / IsAttackDashing이 정확히 반영되게 함.
public class DashState : IState<CharacterMotor>
{
    public enum Kind { Roll, AttackDash }

    readonly Kind kind;
    readonly float dirX;
    readonly float speed;
    readonly float duration;
    float timer;

    public DashState(Kind kind, float dirX, float speed, float duration)
    {
        this.kind = kind;
        this.dirX = dirX;
        this.speed = speed;
        this.duration = duration;
    }

    public void Enter(CharacterMotor motor)
    {
        timer = duration;
        motor.SetDashKind(kind);
    }

    public void Tick(CharacterMotor motor, float dt)
    {
        motor.ApplyHorizontalVelocity(dirX * speed);

        timer -= dt;
        if (timer <= 0f) motor.ChangeState(new MoveState());
    }

    public void Exit(CharacterMotor motor)
    {
        motor.SetDashKind(null);
    }
}
