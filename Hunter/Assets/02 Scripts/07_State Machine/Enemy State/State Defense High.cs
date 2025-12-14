using UnityEngine;

public class StateDefenseHigh : IState
{
    private bool animationPlaying = true;

    public void EnterState(Animator _animator)
    {
        animationPlaying = true;
        _animator.SetTrigger("DefenseHigh");
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
