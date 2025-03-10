using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class Monster : MonoBehaviour
{
    Animator animator;

    Vector3 targetPosition;

    bool isMoving = false;
    bool isJumping = false;
    float speed = 2f;
    int power = 25;

    int layer = 0;

    [SerializeField] Slider slider;
    [SerializeField] MonsterDie MonsterDie;

    int HP = 100;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isMoving && transform.position != targetPosition)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
            if ( transform.position == targetPosition )
            {
                isMoving = false;
                animator.SetBool("IsAttacking", true);
            }
        }
    }

    public void SetIsJumping(bool _value) => isJumping = _value;
    public bool GetIsJumping() => isJumping;


    public void SetTarget(Vector3 _position)
    {
        animator.SetBool("IsAttacking", false);
        isMoving = true;
        targetPosition = _position;
    }

    public void OnAttack()
    {
        Truck.GetInstance().OnDamaged(power);
    }

    public void Jump(Vector3 _position)
    {
        SetTarget(_position);
        layer++;
    }

    public void Fall(Vector3 _position)
    {
        SetTarget(_position);
        layer--;
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
            MonsterManager.GetInstance().MonsterDie(layer, this);

            animator.SetBool("IsDead", true);

            MonsterDie.Die();
        }
    }

    public void SetLayer(int _layer) => layer = _layer;

}
