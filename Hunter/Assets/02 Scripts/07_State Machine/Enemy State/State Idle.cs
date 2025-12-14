using UnityEngine;

public class StateIdle : IState
{
    private EnemyController controller;
    private Animator animator;
    
    private float idleAnimTimer = 0f;
    private float nextIdleAnimTime = 0f;
    private int currentIdleIndex = 0;

    public StateIdle(EnemyController _controller)
    {
        controller = _controller;
    }

    public void EnterState(Animator _animator)
    {
        animator = _animator;

        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }
        
        // 다음 Idle 애니메이션 전환 시간 설정 (3~7초 사이 랜덤)
        nextIdleAnimTime = Random.Range(1f, 4f);
        idleAnimTimer = 0f;
    }

    public void ExitState()
    {
        idleAnimTimer = 0f;
    }

    public void UpdateState()
    {
        // Idle 상태에서 여러 애니메이션 전환
        idleAnimTimer += Time.deltaTime;
        if (idleAnimTimer >= nextIdleAnimTime)
        {
            PlayRandomIdleAnimation();
            nextIdleAnimTime = Random.Range(1f, 4f);
            idleAnimTimer = 0f;
        }

        // 움직여야 하는지 체크
        if (controller.TryToMove())
        {
            controller.ChangeState(EnemyState.MOVE);
            return;
        }

        // 공격 가능한지 체크
        if (controller.CanUseAttack())
        {
            controller.ChangeState(EnemyState.ATTACK);
            return;
        }
    }

    private void PlayRandomIdleAnimation()
    {
        if (animator == null) return;

        int randomIdle = Random.Range(1, 4); // 1~3 사이 랜덤
        Debug.Log($"Idle{randomIdle}");
        animator.SetTrigger("Idle");
        animator.SetTrigger($"Idle{randomIdle}");
        
        currentIdleIndex = randomIdle;
    }
}
