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
            Debug.Log("truck");

            StopCoroutine(Move);
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
