using UnityEngine;

// Grunt의 공격: 제자리에서 공격 애니메이션을 재생하고, 타격 프레임에 베기 이펙트를 재생함.
// 공격 중 여부·쿨타임·공격 끝 처리는 부모(EnemyAttack)가 하고, 여기서는 "어떻게 공격하는지"만 정함.
[RequireComponent(typeof(EffectPlayer))]
public class GruntAttack : EnemyAttack
{
    // 베기 이펙트 정의(DEF_GruntSlash). 인스펙터에서 연결함.
    // grunt 공격 애니메이션의 타격 프레임에 건 Animation Event(OnAttackEffect)에서 재생됨.
    [SerializeField] EffectDefinition slashEffect;

    Animator anim;
    SpriteRenderer spriter;
    EffectPlayer effectPlayer;

    void Awake()
    {
        anim = GetComponent<Animator>();
        spriter = GetComponent<SpriteRenderer>();
        effectPlayer = GetComponent<EffectPlayer>();
    }

    protected override void OnBegin(Enemy enemy)
    {
        anim.SetTrigger("Attack");
    }

    // grunt 공격 애니메이션의 타격 프레임에 등록할 Animation Event용 콜백.
    void OnAttackEffect()
    {
        effectPlayer.Play(slashEffect, spriter.flipX);
    }
}
