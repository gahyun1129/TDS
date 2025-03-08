using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rigidBody;

    bool isAttacking = false;
    float speed = 5f;
    
    float jumpForce = 5f;


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

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if ( !isAttacking )
        {
            if (collision.collider.CompareTag("Zombie") || collision.collider.CompareTag("Truck"))
            {
                StopCoroutine(Move);
                isAttacking = true;
                animator.SetBool("IsAttacking", true);

                MonsterManager.GetInstance().AddMonster(this.gameObject);
            }
        }
    }

    public void OnAttack()
    {
        if (MonsterManager.GetInstance().IsLastMonster(this.gameObject))
        {
            rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
        }
    }

    IEnumerator MonsterMove()
    {
        while(true)
        {
            transform.position += Vector3.left * speed * Time.deltaTime;
            
            yield return null;
        }
    }

}
