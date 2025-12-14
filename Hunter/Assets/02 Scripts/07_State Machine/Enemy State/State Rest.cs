using UnityEngine;

public class StateRest : IState
{
    private EnemyController controller;
    private Animator animator;
    private LevelData levelData;

    private float restStartTime;



    public StateRest(EnemyController _controller)
    {
        controller = _controller;
    }

    public void EnterState(Animator _animator)
    {

    }

    public void ExitState()
    {

    }

    public void UpdateState()
    {

    }
}
