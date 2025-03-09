using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackMonster : MonoBehaviour
{
    private static AttackMonster instance;
    public static AttackMonster GetInstacne() => instance;


    [SerializeField] GameObject bulletPrefab;
    [SerializeField] Transform firePosition;

    Monster target;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    void Start()
    {
        StartCoroutine(Attack());
    }

    
    IEnumerator Attack()
    {

        while(true)
        {
            if (MonsterManager.GetInstance().IsMonsterInList())
            {
                if (target == null)
                {
                    target = MonsterManager.GetInstance().GetFirstMonster();

                }
                if (target != null)
                {
                    Vector3 direction = target.gameObject.transform.position - transform.position; 
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                    Quaternion rotation = Quaternion.Euler(0, 0, angle);
                    transform.rotation = rotation;

                    GameObject bullet = Instantiate(bulletPrefab, firePosition.position, rotation);

                    // Rigidbody2D를 이용해 총알 속도 설정
                    Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
                    rb.AddForce(rotation * Vector2.right * 5f, ForceMode2D.Impulse);

                }
                
            }
            yield return new WaitForSeconds(1f);
        }
    }

    public void SetTargetToNull()
    {
        target = null;
    }
}
