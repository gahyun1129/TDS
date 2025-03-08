using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rigidBody;

    bool isAttacking = false;
    bool isMoving = true;
    float speed = 2f;
    float direction = 1f;
    
    float jumpForce = 5f;
    int curLayer = 0;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position += direction * Vector3.left * speed * Time.deltaTime;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if ( collision.collider.CompareTag("Truck") || collision.collider.CompareTag("Zombie"))
        {
            isMoving = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ( !isAttacking )
        {
            if (collision.collider.CompareTag("Zombie") || collision.collider.CompareTag("Truck"))
            {
                isMoving = false;

                isAttacking = true;
                animator.SetBool("IsAttacking", true);

                curLayer = 0;
                MonsterManager.GetInstance().AddMonster(curLayer, this.gameObject);
            }
        }
        else
        {
            // 0이 아닌 레이어에서 일어나는 일일 것
            if (collision.collider.CompareTag("Truck"))
            {
                if (isMoving)
                {
                    isMoving = false;
                }

                animator.SetBool("IsAttacking", true);
            }
            else if (collision.collider.CompareTag("Zombie") && Mathf.Abs(transform.position.y - collision.transform.position.y) < 0.5f)
            {
                if (isMoving)
                {
                    isMoving = false;
                }
                animator.SetBool("IsAttacking", true);
            }
        }
    }

    public void OnAttack()
    {
        // 꾸준히 데미지는 줌

        if (direction > 0f && MonsterManager.GetInstance().IsLastMonster(curLayer, this.gameObject))
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            
            MonsterManager.GetInstance().MoveMonsterToUpperLayer(curLayer, curLayer + 1, this.gameObject);
            curLayer++;

            animator.SetBool("IsAttacking", false);
            isMoving = true;
        }
        else if (curLayer > 0 && MonsterManager.GetInstance().IsFirstMonster(curLayer, this.gameObject))
        {
            // 밑의 층의 몬스터들 오른쪽으로 살짝 밈

            MonsterManager.GetInstance().MoveMonsters(curLayer - 1);
            MonsterManager.GetInstance().MoveMonsterToLowerLayer(curLayer, curLayer - 1, this.gameObject);
            curLayer--;

        }
    }

    public void MoveRightDirection()
    {
        StartCoroutine(MoveRight());
    }

    IEnumerator MoveRight()
    {
        direction = -1f;

        yield return new WaitForSeconds(0.3f);

        direction = 1f;
    }

}
