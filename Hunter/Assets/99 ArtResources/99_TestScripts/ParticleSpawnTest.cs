using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSpawnTest : MonoBehaviour
{
    /* 
    이 스크립트는 무시해주세요. 
    [애니메이션 키] 대신 [이벤트]로 넣어놓았습니다. 이벤트에 아래 내용을 참고해주세요.
    
    기존 캐릭터에서 애니메이션에서 파티클 껏다켰다 하는 방식 -> 상황에 따라 스폰하는 방식으로 변경
    플레이어가 죽었을 경우 / 죽지 않았을 경우에 따라 파티클이 바뀜. 
    플레이어가 죽었을경우 PTCL_PlayerDead / 공격을 플레이어가 피했을 경우 PTCL_Attack
    */

    [SerializeField] GameObject ptcl; 
    public void AttackParticleSpawn()
    {
        ptcl.SetActive(true);
        Invoke("AttackParticleInactive", 1.5f);
    }
    private void AttackParticleInactive()
    {
        ptcl.gameObject.SetActive(false);
    }
    [SerializeField] Animator animator;

    public void AnimationChange( string _clip )
    {
        if ( this.animator == null ) animator = this.GetComponent<Animator>();
        animator.SetTrigger(_clip);
    }

}
