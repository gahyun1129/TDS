using System.Collections;
using UnityEngine;

public class Health : MonoBehaviour, IDamageable
{
    public bool isDead;
    public float maxHP = 100f;
    public float hp;

    [Header("컴포넌트")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer image;

    private EnemyAutoAI enemyAI;

    public float CurrentHealth => hp;

    public float Defense => 0f;

    void Start()
    {
        hp = maxHP;
        enemyAI = GetComponent<EnemyAutoAI>();
    }

    public void ResetHP()
    {
        hp = maxHP;
        isDead = false;
    }



    public void TakeDamage(float damage)
    {
        if (isDead) return;

        hp -= damage;
        ObjectPoolManager.Instance.ShowDamage(damage.ToString(), transform.position);
        Debug.Log("데미지 " + gameObject.name);

        enemyAI.TriggerKnockback();

        if (hp <= 0)
        {
            isDead = true;
            return;
        }        
    }

}
