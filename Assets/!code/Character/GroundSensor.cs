using System.Collections.Generic;
using UnityEngine;

// 발밑에서 위쪽을 향한 노멀로 접촉 중인 콜라이더 집합으로 접지 여부를 판단.
// 바닥 높이를 가정(예: y<=0.1f)하지 않고, 실제 타일맵/플랫폼 콜라이더와의
// 물리 충돌만으로 판단하기 위함. Player든 Enemy든 물리 기반으로 걷는 캐릭터면 그대로 재사용 가능.
public class GroundSensor : MonoBehaviour
{
    public bool IsGrounded => groundContacts.Count > 0;

    readonly HashSet<Collider2D> groundContacts = new HashSet<Collider2D>();

    void OnCollisionEnter2D(Collision2D collision) => UpdateGroundContact(collision);
    void OnCollisionStay2D(Collision2D collision) => UpdateGroundContact(collision);
    void OnCollisionExit2D(Collision2D collision) => groundContacts.Remove(collision.collider);

    void UpdateGroundContact(Collision2D collision)
    {
        bool touchingFromBelow = false;
        for (int i = 0; i < collision.contactCount; i++)
        {
            if (collision.GetContact(i).normal.y > 0.5f)
            {
                touchingFromBelow = true;
                break;
            }
        }

        if (touchingFromBelow) groundContacts.Add(collision.collider);
        else groundContacts.Remove(collision.collider);
    }
}
