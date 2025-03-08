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

                MonsterManager.GetInstance().AddMonster(0, this);
            }
        }
        else
        {
            if (collision.collider.CompareTag("Truck"))
            {
                if (isMoving)
                {
                    isMoving = false;
                }

                animator.SetBool("IsAttacking", true);
            }
            else if (collision.collider.CompareTag("Zombie") && Mathf.Abs(transform.position.y - collision.transform.position.y) < 0.3f)
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
    }

    public void Jump()
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);

        isMoving = true;

    }

    public void MoveRightDirection(float _speed)
    {
        StartCoroutine(MoveRight(_speed));
    }

    IEnumerator MoveRight(float _speed)
    {

        direction = -1f * _speed;
        animator.SetBool("IsAttacking", false);

        yield return new WaitForSeconds(0.6f);

        direction = 1f * _speed;
        animator.SetBool("IsAttacking", true);

        yield return null;
    }

    public bool GetIsMoving()
    {
        return isMoving;
    }

}
