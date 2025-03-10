using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterDie : MonoBehaviour
{
    public Rigidbody2D head;
    public Rigidbody2D body;
    public Rigidbody2D leftArm;
    public Rigidbody2D rightArm;
    public Rigidbody2D leftLeg;
    public Rigidbody2D rightLeg;

    public float explosionForce = 5f;  // 힘의 크기
    public float torqueForce = 10f;    // 회전력

    public void Die()
    {
        // 부모 오브젝트에서 자식 오브젝트를 분리
        head.transform.parent = null;
        body.transform.parent = null;
        leftArm.transform.parent = null;
        rightArm.transform.parent = null;
        leftLeg.transform.parent = null;
        rightLeg.transform.parent = null;

        EnableRigidbody(head);
        EnableRigidbody(body);
        EnableRigidbody(leftArm);
        EnableRigidbody(rightArm);
        EnableRigidbody(leftLeg);
        EnableRigidbody(rightLeg);

        // 사방으로 흩어지게 힘을 가하기
        ApplyExplosionForce(head);
        ApplyExplosionForce(body);
        ApplyExplosionForce(leftArm);
        ApplyExplosionForce(rightArm);
        ApplyExplosionForce(leftLeg);
        ApplyExplosionForce(rightLeg);

        // 원래 부모 오브젝트 삭제
        Destroy(gameObject);

        Destroy(head.gameObject, 1f);
        Destroy(body.gameObject, 1f);
        Destroy(leftArm.gameObject, 1f);
        Destroy(rightArm.gameObject, 1f);
        Destroy(leftLeg.gameObject, 1f);
        Destroy(rightLeg.gameObject, 1f);
    }

    private void EnableRigidbody(Rigidbody2D rb)
    {
        rb.isKinematic = false;  // 물리적 영향을 받도록 설정
        rb.simulated = true;     // Rigidbody 활성화
    }

    private void ApplyExplosionForce(Rigidbody2D rb)
    {
        Vector2 forceDirection = (rb.position - (Vector2)transform.position).normalized;
        rb.AddForce(forceDirection * explosionForce, ForceMode2D.Impulse);  // 사방으로 튕겨나가게 힘을 추가
        rb.AddTorque(Random.Range(-torqueForce, torqueForce));  // 랜덤한 회전 추가
    }
}
