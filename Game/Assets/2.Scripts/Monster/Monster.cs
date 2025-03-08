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
    
    float jumpForce = 5f;
    int curLayer = 0;

    Coroutine Move;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
    }
    private void Start()
    {
        Move = StartCoroutine(MonsterMove());
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
                //StopCoroutine(Move);
                //Move = null;
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
                    //StopCoroutine(Move);
                    //Move = null;
                    isMoving = false;
                }

                animator.SetBool("IsAttacking", true);
            }
            else if (collision.collider.CompareTag("Zombie") && Mathf.Abs(transform.position.y - collision.transform.position.y) < 0.5f)
            {
                if (isMoving)
                {
                    //StopCoroutine(Move);
                    //Move = null;
                    isMoving = false;
                }

                // animator.SetBool("IsAttacking", true);
            }
        }
    }

    public void OnAttack()
    {
        if (MonsterManager.GetInstance().IsLastMonster(curLayer, this.gameObject))
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
            
            MonsterManager.GetInstance().MoveMonsterToNextLayer(curLayer, curLayer + 1, this.gameObject);
            curLayer++;
            
            if ( !isMoving )
            {
                animator.SetBool("IsAttacking", false);
                //Move = StartCoroutine(MonsterMove());
                isMoving = true;
            }
        }
        else if (curLayer > 0 && MonsterManager.GetInstance().IsFirstMonster(curLayer, this.gameObject))
        {
            // 밑의 층의 몬스터들 오른쪽으로 살짝 밈

            MonsterManager.GetInstance().MoveMonsters(curLayer - 1);
            MonsterManager.GetInstance().MoveMonsterToPrevLayer(curLayer, curLayer - 1, this.gameObject);
            curLayer--;

        }
    }

    IEnumerator MonsterMove()
    {
        while(true)
        {
            if (isMoving)
            {
                transform.position += Vector3.left * speed * Time.deltaTime;
            }
            
            yield return null;
        }
    }

    public void MoveRightDirection()
    {
        transform.position += Vector3.right;
    }

}
