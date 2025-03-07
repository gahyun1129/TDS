using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    float HP = 1f;

    public void OnDamaged(float _damage)
    {
        HP -= _damage;
    }
}
