using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Monster : MonoBehaviour
{
    Animator animator;

    Coroutine Move;

    float speed = 5f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        Move = StartCoroutine(MonsterMove());
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Truck"))
        {
            StopCoroutine(Move);
            animator.SetBool("IsAttacking", true);
        }
    }

    public void OnAttack()
    {
        Debug.Log("АјАн");
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
