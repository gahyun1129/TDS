using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class StateAttack : IState
{
    private EnemyController controller;
    private Animator animator;
    private LevelData levelData;
    
    private bool isMovingToAttack = false;
    private bool isRushing = false;
    private bool isAttacking = false; // 공격 애니메이션 재생 중
    private float rushTimer = 0f;
    private float currentSpeed = 0f;
    private CancellationTokenSource attackCts;

    // 돌진 관련 상수
    private const float BACKWARD_DISTANCE = 0.5f; // 뒤로 물러나는 거리
    private const float BACKWARD_DURATION = 0.2f; // 뒤로 물러나는 시간

    public StateAttack(EnemyController _controller)
    {
        controller = _controller;
        levelData = controller.levelData;
    }
    
    public void EnterState(Animator _animator)
    {
        animator = _animator;
        
        // 상태 초기화
        isMovingToAttack = false;
        isRushing = false;
        isAttacking = false;
        rushTimer = 0f;
        currentSpeed = 0f;

        // 사거리 판단 없이 무조건 뒤로 갔다가 돌진 후 공격
        DashAndAttackAsync().Forget();
    }

    public void ExitState()
    {
        animator.speed = 1f;
        isMovingToAttack = false;
        isRushing = false;
        isAttacking = false; // 공격 플래그 초기화
        rushTimer = 0f;
        currentSpeed = 0f;
        attackCts?.Cancel();
        attackCts?.Dispose();
        attackCts = null;
    }

    public void UpdateState()
    {
        // 공격 애니메이션 재생 중이면 아무것도 하지 않음
        if (isAttacking)
        {
            return;
        }

        if (isMovingToAttack)
        {
            if (isRushing)
            {
                // 돌진 중 - 가속도 적용
                RushTowardsPlayer();
                
                // 사거리 안에 들어오면 공격
                if (!controller.TryToAttack())
                {
                    animator.speed = 1f;
                    isMovingToAttack = false;
                    isRushing = false;
                    attackCts?.Cancel();
                    PerformAttack();
                    return;
                }
            }
        }
    }

    private async UniTaskVoid DashAndAttackAsync()
    {
        attackCts?.Cancel();
        attackCts?.Dispose();
        attackCts = new CancellationTokenSource();

        var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            attackCts.Token,
            controller.GetCancellationTokenOnDestroy()
        );

        try
        {
            // 1단계: 뒤로 물러나기
            Vector3 startPos = controller.transform.position;
            Vector3 backwardPos = startPos + Vector3.right * BACKWARD_DISTANCE;
            
            float backwardTimer = 0f;
            while (backwardTimer < BACKWARD_DURATION && !linkedCts.Token.IsCancellationRequested)
            {
                backwardTimer += Time.deltaTime;
                float t = backwardTimer / BACKWARD_DURATION;
                controller.transform.position = Vector3.Lerp(startPos, backwardPos, t);
                await UniTask.Yield(linkedCts.Token);
            }

            // 2단계: 앞으로 돌진 준비
            isMovingToAttack = true;
            isRushing = true;
            rushTimer = 0f;
            currentSpeed = levelData.enemyBase.speed * levelData.enemyBase.startAccelMultiplier;
            
            if (animator != null)
            {
                animator.speed *= 3f;
                animator.SetTrigger("Moving");
            }

            // 돌진 루프는 UpdateState에서 처리
            while (isMovingToAttack && !linkedCts.Token.IsCancellationRequested)
            {
                await UniTask.Yield(linkedCts.Token);
            }
        }
        catch (System.OperationCanceledException)
        {
            isMovingToAttack = false;
            isRushing = false;
        }
    }

    private void RushTowardsPlayer()
    {
        // 가속도 적용
        rushTimer += Time.deltaTime;
        
        if (rushTimer < levelData.enemyBase.accelerationTime)
        {
            // 가속 구간: 지수 곡선으로 급격하게 가속 (처음만 느리고 바로 슝!)
            float t = rushTimer / levelData.enemyBase.accelerationTime;
            
            // EaseOutQuad: 빠르게 가속 후 부드럽게 감속
            // 또는 EaseOutCubic: 더 격하게 빠른 가속
            float easedT = 1f - Mathf.Pow(1f - t, 3f); // Cubic easing for aggressive acceleration
            
            float startSpeed = levelData.enemyBase.speed * levelData.enemyBase.startAccelMultiplier;
            float targetSpeed = levelData.enemyBase.speed * levelData.enemyBase.attackSpeedMultiplier;
            
            currentSpeed = Mathf.Lerp(startSpeed, targetSpeed, easedT);
        }
        else
        {
            // 최고 속도 유지
            currentSpeed = levelData.enemyBase.speed * levelData.enemyBase.attackSpeedMultiplier;
        }

        // 이동
        controller.transform.position += Vector3.left * currentSpeed * Time.deltaTime;
    }

    private void PerformAttack()
    {
        isAttacking = true; // 공격 애니메이션 시작
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        // OnAnimationEnd()가 호출되면 자동으로 Idle로 전환됨
    }
}
