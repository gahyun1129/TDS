using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Box : MonoBehaviour
{
    [SerializeField] Slider slider;
    [SerializeField] int maxHP = 1000;

    int HP = 1000;

    bool isBroken = false;

    private void Start()
    {
        HP = maxHP; 
    }

    public void OnDamaged(int _damage)
    {

        if (HP == maxHP)
        {
            slider.gameObject.SetActive(true);
        }

        HP -= _damage;
        slider.value = HP / (float)maxHP;

        if (HP <= 0)
        {
            isBroken = true;
        }
    }

    public bool IsBroken() => isBroken;
}
