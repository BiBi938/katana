using UnityEngine;

// 이펙트 프리팹에 붙여서 재생이 끝나면 풀에 반환되게 함.
// 우선순위: 1) 이펙트 자신이 끝을 알림(Animation Event로 Release() 호출, 또는 ParticleSystem Stop Action=Callback)
//          2) 그래도 안 불리면 lifetime 안전장치가 강제로 반환.
public class EffectInstance : MonoBehaviour
{
    float safetyTimer;
    bool countingDown;
    bool released;

    public void Begin(float safetyLifetime)
    {
        released = false;
        countingDown = safetyLifetime > 0f;
        safetyTimer = safetyLifetime;
    }

    // Animation Event에서 이 이름으로 호출 (클립 마지막 프레임에 등록).
    public void Release()
    {
        if (released) return;
        released = true;
        countingDown = false;
        EffectPoolManager.Instance.Release(gameObject);
    }

    // ParticleSystem의 Stop Action을 Callback으로 설정해두면 Unity가 정지 시 자동으로 호출해줌.
    void OnParticleSystemStopped()
    {
        Release();
    }

    void Update()
    {
        if (!countingDown) return;

        safetyTimer -= Time.deltaTime;
        if (safetyTimer <= 0f) Release();
    }
}
