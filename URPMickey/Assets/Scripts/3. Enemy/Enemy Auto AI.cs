using System.Collections;
using UnityEngine;

public enum BattleState
{
    Idle,
    Moving,
    Attacking,
    Repositioning,
    Damaged,
    Dead
}

public class EnemyAutoAI : MonoBehaviour
{
    public BattleState currentState = BattleState.Idle;
    private BattleState previousState; // ★ 넉백 직전의 상태를 저장

    [Header("타켓팅")]
    [SerializeField] private Transform player;
    [SerializeField] private string targetTag = "Player";

    [Header("전투 스탯")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float minRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private float knockbackForce = 5f;     // ★ 넉백 힘
    [SerializeField] private float knockbackDuration = 0.1f; // ★ 넉백 지속 시간

    [Header("탐지 범위")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float targetSearchInterval = 0.2f;

    private Animator anim;
    private Health health;
    private Rigidbody2D rb;
    private float lastAttackTime = -999f;
    private float lastTargetSearchTime = -999f;
    private bool isDying = false;
    private float knockbackTimer = 0f; // ★ 넉백 시간 카운터
    private bool isDamaged = false;

    private const string ANIM_IS_MOVING = "isMoving";
    private const string ANIM_ATTACK_TRIGGER = "Attack";
    private const string ANIM_IS_DEAD = "isDead";

    void Start()
    {
        health = GetComponent<Health>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();

        FindTarget();
    }

    void OnDisable()
    {
        ResetMemory();
    }

    public void ResetMemory()
    {
        currentState = BattleState.Idle;
        previousState = BattleState.Idle;

        anim.SetBool(ANIM_IS_DEAD, false);
        GetComponent<Collider2D>().enabled = true;

        isDamaged = false;
        isDying = false;

        knockbackTimer = 0f;
        lastAttackTime = -999f;
        lastTargetSearchTime = -999f;
        
        health.ResetHP();
        FindTarget();
    }

    public void SetIsDamaged(bool _isDamaged) => isDamaged = _isDamaged;

    // ★ Health.cs에서 이 함수를 호출하여 넉백을 트리거합니다.
    // (기존 IsDamaged 함수를 대체)
    public void TriggerKnockback()
    {
        // 이미 죽었거나 넉백 중일 때는 넉백을 중첩하지 않음
        if (currentState == BattleState.Dead || currentState == BattleState.Damaged) 
        {
            return;
        }

        if (isDamaged) return;

        
        // 넉백 직전의 상태를 저장 (Idle, Moving, Attacking 등)
        previousState = currentState;
        
        // 넉백 타이머 설정
        knockbackTimer = knockbackDuration;
        
        // 넉백 상태로 강제 전환
        ChangeState(BattleState.Damaged);
    }
        


    void Update()
    {
        if (BattleManager.Instance.CurGameState == GameState.GameOver) return;

        if (health.isDead && !isDying)
        {
            ChangeState(BattleState.Dead);
        }

        if (isDying) return;

        if (isDamaged) return;

        switch (currentState)
        {
            case BattleState.Idle:
                UpdateIdle();
                break;
            case BattleState.Moving:
                UpdateMoving();
                break;
            case BattleState.Attacking:
                UpdateAttacking();
                break;
            case BattleState.Repositioning:
                UpdateRepositioning();
                break;
            case BattleState.Damaged: // ★ 넉백 상태 업데이트
                UpdateDamaged();
                break;

        }
        
    }

    // -------------------------------------------------------------------
    // 상태별 업데이트 함수
    // -------------------------------------------------------------------

    void UpdateIdle()
    {
        rb.velocity = Vector2.zero;

        if (Time.time > lastTargetSearchTime + targetSearchInterval / TimeManager.Instance.TimeScale)
        {
            FindTarget();
        }

        if (player != null)
        {
            ChangeState(BattleState.Moving);
        }
    }

    void UpdateMoving()
    {
        if (!player) return; // 타겟 유효성 검사

        float distance = Vector2.Distance(transform.position, player.position);

        // [전환 조건 1] 공격 범위에 도달하면 -> Attacking
        if (distance <= attackRange)
        {
            ChangeState(BattleState.Attacking);
        }
        else // [행동] 타겟을 향해 이동
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            FaceTarget(direction);
        }
    }

    void UpdateAttacking()
    {
        if (!player) return; // 타겟 유효성 검사

        rb.velocity = Vector2.zero; // 공격 중엔 멈춤
        float distance = Vector2.Distance(transform.position, player.position);
        FaceTarget((player.position - transform.position).normalized);

        // [전환 조건 1] 타겟이 공격 범위를 벗어나면 -> Moving (즉시 추격)
        if (distance > attackRange)
        {
            ChangeState(BattleState.Moving);
            return;
        }

        // [전환 조건 2] 타겟이 너무 가까우면 -> Repositioning (즉시 후퇴)
        if (distance < minRange)
        {
            ChangeState(BattleState.Repositioning);
            return;
        }

        // [행동] 공격 쿨타임이 되면 공격 실행
        if (Time.time > lastAttackTime + attackCooldown / TimeManager.Instance.TimeScale)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    void UpdateRepositioning()
    {
        if (!player) return; // 타겟 유효성 검사

        float distance = Vector2.Distance(transform.position, player.position);

        // [전환 조건 1] 적정 거리(minRange) 이상 벌어졌다면 -> Attacking
        if (distance > minRange)
        {
            ChangeState(BattleState.Attacking);
        }
        // [전환 조건 2] 너무 멀리 도망가서 공격 범위를 벗어나면 -> Moving (다시 추격)
        else if (distance > attackRange)
        {
            ChangeState(BattleState.Moving);
        }
        else // [행동] 타겟 반대 방향으로 이동
        {
            Vector2 direction = (transform.position - player.position).normalized;
            rb.velocity = direction * moveSpeed;
            FaceTarget(-direction); // 타겟을 바라보게 하거나 (FaceTarget(direction * -1)), 등을 보이게 (FaceTarget(direction))
        }
    }
    // ★ 넉백 상태일 때 실행되는 함수
    void UpdateDamaged()
    {
        if (knockbackTimer > 0)
        {
            // 넉백 시간 감소 (Time.deltaTime은 Time.timeScale의 영향을 받음)
            knockbackTimer -= TimeManager.Instance.DeltaTime;
        }
        else
        {
            // 넉백 시간이 끝나면 '이전 상태'로 복귀
            ChangeState(previousState);
        }
    }

    void UpdateDead()
    {
        if (isDying) return;
        isDying = true;

        BattleManager.Instance.TrySpawnRune(transform.position);

        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        Vector2 knockbackDir = (transform.position - player.position).normalized;
        rb.velocity = knockbackDir * knockbackForce;
        FaceTarget(-knockbackDir);

        Invoke("OnDead", 1);
    }

    // -------------------------------------------------------------------
    // 보조 함수
    // -------------------------------------------------------------------

    void ChangeState(BattleState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        if (anim == null) return; 

        switch (newState)
        {
            case BattleState.Idle:
                anim.SetBool(ANIM_IS_MOVING, false);
                break;
            case BattleState.Moving:
                anim.SetBool(ANIM_IS_MOVING, true);
                break;
            case BattleState.Attacking:
                // 공격 상태 진입 시 쿨타임을 바로 돌리지 않고,
                // 첫 공격은 바로 나가도록 하거나 (lastAttackTime = Time.time - attackCooldown;)
                 // 쿨타임 후 나가도록 할 수 있음 (lastAttackTime = Time.time;)
                anim.SetBool(ANIM_IS_MOVING, false);
                break;
            case BattleState.Repositioning:
                anim.SetBool(ANIM_IS_MOVING, true); // 후퇴도 움직임
                break;
            case BattleState.Damaged: // ★ 넉백 상태 진입 시
                // anim.SetTrigger(ANIM_HIT_TRIGGER); // 피격 애니메이션
                
                // 넉백 방향 계산 및 힘 적용
                if (player != null)
                {
                    Vector2 knockbackDir = (transform.position - player.position).normalized;
                    rb.velocity = knockbackDir * knockbackForce; // ★ 즉시 넉백 적용
                    FaceTarget(-knockbackDir); // 등을 돌림
                }
                else
                {
                    rb.velocity = Vector2.zero; // 플레이어가 없으면 그냥 멈춤
                }
                break;
            case BattleState.Dead:
                anim.SetBool(ANIM_IS_MOVING, false);
                anim.SetBool(ANIM_IS_DEAD, true);
                UpdateDead(); // 죽음 로직 즉시 실행
                break;
        }

    }

    void FindTarget()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag(targetTag);
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    void FaceTarget(Vector2 direction)
    {
        if (direction.x > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }

    void PerformAttack()
    {
        if (player == null) return;

        // ★★★ 애니메이션 트리거 실행 ★★★
        if (anim != null)
        {
            anim.SetTrigger(ANIM_ATTACK_TRIGGER);
        }

        // TODO: 여기에 공격 애니메이션 또는 사운드 재생

        InGamePlayerStat targetHealth = player.GetComponent<InGamePlayerStat>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(attackDamage);
        }
    }
    
    void OnDead()
    {
        BattleManager.Instance.DeleteEnemy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // 탐지 범위
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red; // 공격 범위
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.blue; // 최소 거리 (후퇴)
        Gizmos.DrawWireSphere(transform.position, minRange);
    }
    
    public void ExplosionEnemy()
    {
        ChangeState(BattleState.Dead);
    }
}