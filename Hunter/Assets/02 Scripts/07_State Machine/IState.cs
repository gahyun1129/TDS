using UnityEngine;

public interface IState
{
    void EnterState(Animator animator);
    void UpdateState();
    void ExitState();
}
