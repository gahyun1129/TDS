using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorTest : MonoBehaviour
{
    [SerializeField] Animator animator;


    public void AnimPlay( string _clipName )
    {
        animator.Play(_clipName);
    }
}
