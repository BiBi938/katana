using UnityEngine;

[CreateAssetMenu(fileName = "New Effect", menuName = "Effects/Effect Definition")]
public class EffectDefinition : ScriptableObject
{
    public GameObject prefab;

    // EffectPlayer에 등록된 소켓 이름과 매칭됨. 못 찾으면 캐릭터 위치를 그대로 사용.
    public string socketName = "Center";
    public Vector2 localOffset;

    // 이펙트가 스스로 안 끝내면(Animation Event/ParticleSystem Stop Action 설정을 깜빡한 경우 등) 강제로 반환할 안전장치용 최대 수명(초).
    // 0 이하면 강제 종료 없음 — 이펙트가 반드시 스스로 Release()를 호출해야 함.
    public float lifetime = 1f;

    // Project Settings > Tags and Layers > Sorting Layers에 미리 만들어둔 레이어 이름과 정확히 일치해야 함.
    // 존재하지 않는 이름을 넣으면 Unity가 경고를 띄우고 Default로 되돌림.
    public string sortingLayerName = "Effect";
    public int sortingOrder = 10;

    // true면 스폰 후 소켓(부모)의 자식으로 붙어서 캐릭터가 움직여도 같이 따라감 (근접 공격 이펙트 등).
    // false면 스폰된 위치에 독립적으로 남아서 자기 힘으로 움직임 (총알 등 투사체).
    public bool followSocket = true;
}
