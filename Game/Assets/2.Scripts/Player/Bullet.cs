using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{

    [SerializeField] int damage = 30;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        else if (collision.collider.CompareTag("Zombie"))
        {
            Debug.Log("しししし");
            collision.gameObject.GetComponent<Monster>().OnDamaged(damage);
            Destroy(gameObject);
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
        else if (collision.CompareTag("Zombie"))
        {
            Debug.Log("ssss");
            collision.gameObject.GetComponent<Monster>().OnDamaged(damage);
            Destroy(gameObject);
        }
    }

}
