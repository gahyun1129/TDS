using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TruckMove : MonoBehaviour
{
    [SerializeField] float speed = 200f;

    GameObject backWheel;
    GameObject frontWheel;

    private void Start()
    {
        backWheel = transform.GetChild(0).gameObject;
        frontWheel = transform.GetChild(1).gameObject;
    }
    private void Update()
    {
        backWheel.transform.Rotate(0, 0, -speed * Time.deltaTime);
        frontWheel.transform.Rotate(0, 0, -speed * Time.deltaTime);
    }
}
