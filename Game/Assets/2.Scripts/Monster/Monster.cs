using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rigidBody;

    bool isAttacking = false;
    bool isMoving = true;
    bool isJumping = false;

    float speed = 2f;
    float jumpForce = 5f;

    float xDirection = 1f;

    int curLayer = 0;
    float floorOffset = -3.2f;

    [SerializeField] Slider slider;
    int HP = 100;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isMoving)
        {
            transform.position += xDirection * Vector3.left * speed * Time.deltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.collider.CompareTag("Truck") )
        {
            isMoving = false;
            animator.SetBool("IsAttacking", true);

            if (!isAttacking)
            {
                isAttacking = true;
                MonsterManager.GetInstance().AddMonster(curLayer, this);
            }
        }
        else if (collision.collider.CompareTag("Zombie") && Mathf.Abs(transform.position.y - collision.transform.position.y) < 0.3f)
        {
            isMoving = false;
            animator.SetBool("IsAttacking", true);

            if (!isAttacking)
            {
                isAttacking = true;
                MonsterManager.GetInstance().AddMonster(curLayer, this);
            }
        }
    }

    public void OnAttack()
    {
        if ( xDirection > 0f)
        {
            if (curLayer < 1 && MonsterManager.GetInstance().AmILastMonsterInLayer(curLayer, this))
            {
                animator.SetBool("IsAttacking", false);
                isMoving = true;

                MonsterManager.GetInstance().RemoveMonster(curLayer, this);
                StartCoroutine(Jumping());
            }
            else if ( MonsterManager.GetInstance().AmIFirstMonsterInLayer(curLayer, this))
            {
                MonsterManager.GetInstance().ManageMonster(curLayer);
                curLayer--;
            }
        }
    }

    IEnumerator Jumping()
    {
        rigidBody.velocity = new Vector2(rigidBody.velocity.x, jumpForce);
        curLayer++;

        while (true)
        {
            if ( transform.position.y >= floorOffset + curLayer * 0.8f)
            {
                MonsterManager.GetInstance().AddMonster(curLayer, this);
                break;
            }
            yield return null;
        }
    }
    public bool GetIsMoving()
    {
        return isMoving;
    }

    public void MoveRightDirection(float _speed)
    {
        StartCoroutine(MoveRight(_speed));
    }

    IEnumerator MoveRight(float _speed)
    {

        isMoving = true;
        xDirection = -1f * _speed;
        animator.SetBool("IsAttacking", false);

        yield return new WaitForSeconds(0.6f);

        xDirection = 1f * _speed;

        yield return new WaitForSeconds(0.6f);


        isMoving = false;

    }

    public void OnDamaged(int damage)
    {
        if (HP == 100)
        {
            slider.gameObject.SetActive(true);
        }

        HP -= damage;

        slider.value = HP / 100f;

        if ( HP < 0)
        {
            AttackMonster.GetInstacne().SetTargetToNull();
            MonsterManager.GetInstance().MonsterDie(this);

            animator.SetBool("IsDead", true);

            Destroy(gameObject);
        }
    }

}
