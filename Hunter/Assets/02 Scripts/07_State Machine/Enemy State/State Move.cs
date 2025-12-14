using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StateMove : IState
{   
    private EnemyController controller;
    private Animator animator;
    private LevelData levelData;

    private bool isMoving = false;
    private float moveStartTime = 0f;
    private float accumulatedMoveTime = 0f;
    private CancellationTokenSource moveCts;
    private CancellationTokenSource restCts;
        
    public StateMove(EnemyController _controller)
    {
        controller = _controller;
        levelData = controller.levelData;
    }

    public void EnterState(Animator _animator)
    {
        animator = _animator;

        if (controller.TryToMove())
        {
            PrepareToMoveAsync().Forget();
        }
    }

    public void ExitState()
    {
        isMoving = false;
        
        // moveCts 정리
        moveCts?.Cancel();
        moveCts?.Dispose();
        moveCts = null;
        
        // restCts 정리
        restCts?.Cancel();
        restCts?.Dispose();
        restCts = null;
    }

    public void UpdateState()
    {
        if (isMoving)
        {
            // 더 이상 움직일 필요가 없으면 Idle로 전환
            if (!controller.TryToMove())
            {
                isMoving = false;
                moveCts?.Cancel();
                accumulatedMoveTime = 0f;
                controller.ChangeState(EnemyState.IDLE);
                return;
            }

            // 플레이어가 움직이는 동안 추적 시간 누적
            if (controller.player != null && controller.player.IsMoving())
            {
                accumulatedMoveTime += Time.deltaTime;
                
                // 추적 시간이 끝나면 휴식
                if (accumulatedMoveTime >= levelData.enemyBase.chaseDuration)
                {
                    isMoving = false;
                    accumulatedMoveTime = 0f;
                    RestAndRepeatAsync().Forget();
                    return;
                }
            }

            MoveTowardsPlayer();
        }
    }

    private void MoveTowardsPlayer()
    {
        float elapsedTime = Time.time - moveStartTime;

        float speedMultiplier;
        if (elapsedTime < levelData.enemyBase.accelerationTime)
        {
            float t = elapsedTime / levelData.enemyBase.accelerationTime;
            speedMultiplier = Mathf.Lerp(levelData.enemyBase.startAccelMultiplier, 1f, t);
        }
        else
        {
            speedMultiplier = 1f;
        }

        controller.transform.position += Vector3.left * levelData.enemyBase.speed * speedMultiplier * Time.deltaTime;
    }

    private async UniTaskVoid PrepareToMoveAsync()
    {
        moveCts?.Cancel();
        moveCts?.Dispose();
        moveCts = new CancellationTokenSource();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            moveCts.Token,
            controller.GetCancellationTokenOnDestroy()
        );

        try
        {
            await UniTask.Delay((int)(levelData.enemyBase.reactionDelay * 1000), cancellationToken: linkedCts.Token);

            if (controller.TryToMove())
            {
                isMoving = true;
                moveStartTime = Time.time;

                if (animator != null)
                {
                    animator.SetTrigger("Moving");
                }
            }
        }
        catch (System.OperationCanceledException)
        {
            isMoving = false;
        }
    }

    private async UniTaskVoid RestAndRepeatAsync()
    {
        // 멤버 변수 restCts 사용
        restCts?.Cancel();
        restCts?.Dispose();
        restCts = new CancellationTokenSource();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            restCts.Token,
            controller.GetCancellationTokenOnDestroy()
        );

        try
        {
            controller.sweatEffect.SetActive(true);
            if (animator != null)
            {
                animator.SetTrigger("Idle");
            }

            await UniTask.Delay((int)(levelData.enemyBase.restDuration * 1000), cancellationToken: linkedCts.Token);

            controller.sweatEffect.SetActive(false);

            if (controller.TryToMove())
            {
                PrepareToMoveAsync().Forget();
            }
            else
            {
                controller.ChangeState(EnemyState.IDLE);
            }
        }
        catch (System.OperationCanceledException)
        {
        }
        finally
        {
            linkedCts?.Dispose();
        }
    }
}
