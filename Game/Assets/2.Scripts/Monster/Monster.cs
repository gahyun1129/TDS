using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    Animator animator;
    Rigidbody2D rigidBody;

    Vector3 targetPosition;

    bool isAttacking = false;
    bool isMoving = false;
    bool isJumping = false;
    float speed = 2f;
    float direction = 1f;
    
    float jumpForce = 5f;

    int curLayer = 0;

    [SerializeField] Slider slider;
    int HP = 100;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        rigidBody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isMoving && transform.position != targetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if ( transform.position == targetPosition )
            {
                isMoving = false;
                // animator.SetBool("IsAttacking", true);
            }
        }
    }

    public void SetIsJumping(bool _value) => isJumping = _value;
    public bool GetIsJumping() => isJumping;


    public void SetTarget(Vector3 _position)
    {
        isMoving = true;
        targetPosition = _position;
    }

    public void OnAttack()
    {
        // Damage To Truck
    }

    public void Jump(Vector3 _position)
    {
        SetTarget(_position);
        curLayer++;
    }

    public void Fall(Vector3 _position)
    {
        SetTarget(_position);
        curLayer--;
    }

    public bool GetIsMoving()
    {
        return isMoving;
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
            MonsterManager.GetInstance().MonsterDie(curLayer, this);

            animator.SetBool("IsDead", true);
            Destroy(gameObject);
        }
    }

}
