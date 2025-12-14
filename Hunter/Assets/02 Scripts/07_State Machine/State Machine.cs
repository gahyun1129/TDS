using UnityEngine;

public class StateMachine
{
    public IState CurrentState { get; private set; }
    
    private Animator animator;

    public StateMachine(IState defaultState, Animator anim)
    {
        CurrentState = defaultState;
        animator = anim;
    }

    public void SetState(IState state)
    {
        CurrentState.ExitState();

        CurrentState = state;

        CurrentState.EnterState(animator);
    }

    public void DoState()
    {
        CurrentState.UpdateState();
    }
}
