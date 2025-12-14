using UnityEngine;

public class AnimationEvents : MonoBehaviour
{
    [SerializeField] private EnemyController controller;
    [SerializeField] private Animator animator;

    public void OnAnimationEnd()
    {
        controller.ChangeState(EnemyState.IDLE);
    }

    public void OnAttackStart()
    {
        animator.speed = 1f;
        Debug.Log("공격 시작");
        InGameManager.Instance.canAttack = true;
    }

    public void OnAttackEnd()
    {
        Debug.Log("공격 끝");
        InGameManager.Instance.canAttack = false;
        
        // 공격 쿨타임 시작
        controller.StartCooldown(EnemyState.ATTACK);
    }

    public void OnDefenseLowEnd()
    {
        Debug.Log("하단 방어 끝");
        
        // 하단 방어 쿨타임 시작
        controller.StartCooldown(EnemyState.DEFENSE_LOW);
    }

    public void OnDefenseHighEnd()
    {
        Debug.Log("상단 방어 끝");
        
        // 상단 방어 쿨타임 시작
        controller.StartCooldown(EnemyState.DEFENSE_HIGH);
    }
    
    public void AttackParticleSpawn()
    {
        EffectManager.Instance.ShakingCam(controller.levelData.cameraShaking.duration, controller.levelData.cameraShaking.magnitude);
    }
}
