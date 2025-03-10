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

    public float explosionForce = 5f;  // ���� ũ��
    public float torqueForce = 10f;    // ȸ����

    public void Die()
    {
        // �θ� ������Ʈ���� �ڽ� ������Ʈ�� �и�
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

        // ������� ������� ���� ���ϱ�
        ApplyExplosionForce(head);
        ApplyExplosionForce(body);
        ApplyExplosionForce(leftArm);
        ApplyExplosionForce(rightArm);
        ApplyExplosionForce(leftLeg);
        ApplyExplosionForce(rightLeg);

        // ���� �θ� ������Ʈ ����
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
        rb.isKinematic = false;  // ������ ������ �޵��� ����
        rb.simulated = true;     // Rigidbody Ȱ��ȭ
    }

    private void ApplyExplosionForce(Rigidbody2D rb)
    {
        Vector2 forceDirection = (rb.position - (Vector2)transform.position).normalized;
        rb.AddForce(forceDirection * explosionForce, ForceMode2D.Impulse);  // ������� ƨ�ܳ����� ���� �߰�
        rb.AddTorque(Random.Range(-torqueForce, torqueForce));  // ������ ȸ�� �߰�
    }
}
