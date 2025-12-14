using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateDead : IState
{
    private EnemyController controller;
    private Animator animator;

    public StateDead(EnemyController _controller)
    {
        controller = _controller;
    }

    public void EnterState(Animator animator)
    {
        throw new System.NotImplementedException();
    }

    public void ExitState()
    {
        throw new System.NotImplementedException();
    }

    public void UpdateState()
    {
        throw new System.NotImplementedException();
    }
}

