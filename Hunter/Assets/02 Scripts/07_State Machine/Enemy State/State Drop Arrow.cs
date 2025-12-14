using UnityEngine;

public class StateDropArrow : IState
{
    private EnemyController controller;
    private Animator animator;
    private bool animationPlaying = true;

    public StateDropArrow(EnemyController _controller)
    {
        controller = _controller;
    }

    public void EnterState(Animator _animator)
    {
        animator = _animator;
        animationPlaying = true;
        
        animator.SetTrigger("DropArrow");
        
        controller.DropAllArrows();
    }

    public void ExitState()
    {
        animationPlaying = false;
    }

    public void UpdateState()
    {
        // 애니메이션이 끝날 때까지 대기
        // OnAnimationEnd()가 호출되면 자동으로 Idle로 전환됨
    }
}
