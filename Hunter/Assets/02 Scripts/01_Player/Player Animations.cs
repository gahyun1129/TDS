using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimations : MonoBehaviour
{
    [SerializeField] private BearController controller;
    [SerializeField] private Animator animator;

    // shooting
    public void OnShootEnd()
    {
        controller.ChangeState(PlayerState.IDLE);
    }
    
    public void OnStartDead()
    {
        Debug.Log("@@@ Dead Trigger 동작!!");
    }

}
