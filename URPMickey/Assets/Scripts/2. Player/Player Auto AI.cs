using UnityEngine;

public enum PlayerBattleState
{
    Idle,
    Moving,
    Attacking,
    Dead
}

public class PlayerAutoAI : MonoBehaviour
{
    public PlayerBattleState currentState = PlayerBattleState.Idle;

    [Header("타겟팅")]
    [SerializeField] private Transform currentTarget;
    [SerializeField] private LayerMask targetLayer; 
    
    [Header("전투 스탯")]
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float attackRange = 5f;     
    
    [Header("탐지 범위")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float targetSearchInterval = 0.2f;

    private InGamePlayerStat stat;
    private Rigidbody2D rb;
    private Animator anim;
    private SkillManager skillManager; // SkillManager 참조
    private float lastTargetSearchTime = -999f;
    private bool isDying = false;

    private const string ANIM_IS_MOVING = "isMoving";
    private const string ANIM_IS_DEAD = "isDead";

    void Start()
    {
        stat = GetComponent<InGamePlayerStat>();
        rb = GetComponent<Rigidbody2D>();
        skillManager = GetComponent<SkillManager>();
        anim = GetComponent<Animator>();

        targetLayer = LayerMask.GetMask("Enemy"); // 적 레이어 자동 설정

        BattleManager.Instance.OnGameReady += ResetPlayer;
    }

    void OnDestroy()
    {
        BattleManager.Instance.OnGameReady -= ResetPlayer;
    }

    public void ResetPlayer(int stage)
    {
        transform.position = new Vector3(0, 0, 0);
        anim.SetBool(ANIM_IS_MOVING, false);
        lastTargetSearchTime = -999f;
        currentTarget = null;
    }

    void Update()
    {
        if (BattleManager.Instance.CurGameState == GameState.GameOver) return;

        if (stat.IsDead && !isDying)
        {
            ChangeState(PlayerBattleState.Dead);
        }

        if (isDying) return;

        skillManager.currentTarget = currentTarget != null ? currentTarget.gameObject : null;

        switch (currentState)
        {
            case PlayerBattleState.Idle:
                UpdateIdle();
                break;
            case PlayerBattleState.Moving:
                UpdateMoving();
                break;
            case PlayerBattleState.Attacking:
                UpdateAttacking();
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

        if (currentTarget != null)
        {
            ChangeState(PlayerBattleState.Moving);
        }
    }

    void UpdateMoving()
    {
        if (!IsTargetValid()) return;

        float distance = Vector2.Distance(transform.position, currentTarget.position);

        if (distance <= attackRange)
        {
            ChangeState(PlayerBattleState.Attacking);
        }
        else
        {
            Vector2 direction = (currentTarget.position - transform.position).normalized;
            rb.velocity = direction * moveSpeed;
            FaceTarget(direction);
        }
    }

    void UpdateAttacking()
    {
        if (!IsTargetValid()) return;

        rb.velocity = Vector2.zero;
        float distance = Vector2.Distance(transform.position, currentTarget.position);
        FaceTarget((currentTarget.position - transform.position).normalized);

        // [전환 조건 1] 타겟이 공격 범위를 벗어나면 -> Moving (즉시 추격)
        if (distance > attackRange)
        {
            ChangeState(PlayerBattleState.Moving);
            return;
        }

        // [행동] SkillManager를 통해 스킬 사용 시도
        // 쿨타임이 돌면 0번 스킬 사용, 안되면 1번 스킬 시도...
        // (가장 간단한 자동 스킬 로직)
        for (int i = 0; i < skillManager.equippedSkills.Count; i++)
        {
            // UseSkill이 쿨타임/마나 등을 체크하고 성공하면 true를 반환
            if (skillManager.UseSkill(i))
            {
                break; // 스킬 하나 사용했으면 이번 프레임은 종료
            }
        }
    }

    void UpdateDead()
    {
        if (isDying) return;
        isDying = true;

        Debug.Log("플레이어 사망");
        rb.velocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        skillManager.OnGameEnd();

        BattleManager.Instance.GameEnd(!isDying);
    }
    
    // -------------------------------------------------------------------
    // 보조 함수 (AutoBattleAI와 거의 동일)
    // -------------------------------------------------------------------

    bool IsTargetValid()
    {
        if (currentTarget == null)
        {
            ChangeState(PlayerBattleState.Idle);
            return false;
        }

        Health targetHealth = currentTarget.GetComponent<Health>();
        if (targetHealth != null && targetHealth.isDead)
        {
            currentTarget = null;
            ChangeState(PlayerBattleState.Idle);
            return false;
        }
        return true;
    }

    void ChangeState(PlayerBattleState newState)
    {
        if (currentState == newState) return;
        currentState = newState;

        if (anim == null) return; // Animator 없으면 중단

        // 새로운 상태에 맞춰 애니메이션 파라미터 설정
        switch (newState)
        {
            case PlayerBattleState.Idle:
                anim.SetBool(ANIM_IS_MOVING, false);
                break;
            case PlayerBattleState.Moving:
                anim.SetBool(ANIM_IS_MOVING, true);
                break;
            case PlayerBattleState.Attacking:
                anim.SetBool(ANIM_IS_MOVING, false);
                break;
            case PlayerBattleState.Dead:
                anim.SetBool(ANIM_IS_MOVING, false);
                anim.SetBool(ANIM_IS_DEAD, true);
                UpdateDead(); // 죽음 로직 즉시 실행
                break;
        }
    }

    void FindTarget()
    {
        lastTargetSearchTime = Time.time;
        
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRange, targetLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestTarget = null;

        foreach (var col in colliders)
        {
            Health targetHealth = col.GetComponent<Health>();
            if (targetHealth != null && !targetHealth.isDead)
            {
                float distance = Vector2.Distance(transform.position, col.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = col.transform;
                }
            }
        }
        currentTarget = closestTarget;
    }

    void FaceTarget(Vector2 direction)
    {
        if (direction.x > 0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (direction.x < -0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }



    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // 탐지 범위
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red; // 공격 범위
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}