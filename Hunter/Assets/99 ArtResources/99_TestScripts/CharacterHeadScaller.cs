using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterHeadScaller : MonoBehaviour
{
    bool isLookingAhead, lookAhead;
    [SerializeField] Transform headBone;
    void OnEnable()
    {
        isLookingAhead = true;
        lookAhead = true;
    }
    public void LookAhead()
    {
        isLookingAhead = true;
    }
    public void LookBack()
    {
        isLookingAhead = false;
    }

    void LateUpdate()
    {
        if ( lookAhead == isLookingAhead ) return;

        float _y = isLookingAhead ? 1f : -1f;
        headBone.localScale = new Vector3 ( 1f, _y, 1f );

        lookAhead = isLookingAhead;
    }
}
