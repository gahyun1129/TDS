using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine.Pool;
using System.Threading;
using Solo.MOST_IN_ONE;

[RequireComponent(typeof(Rigidbody2D))]
public class SpearProjectile : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [Tooltip("실제로 꽂힐 기준점 (새의 부리 끝 위치)")]
    [SerializeField] private Transform tipPoint;

    [Header("Layers")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask blockLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask bounceLayer;
    [SerializeField] private LayerMask weakLayer;

    // 내부 변수
    private Rigidbody2D rb;
    [SerializeField] private Collider2D bodyCol;
    [SerializeField] private Collider2D mouseCol;

    private float _baseGravityScale;
    private float _fallMultiplier;

    // 상태 플래그
    private bool isStuck = false;     // 박혀서 멈춘 상태
    private bool isPhysicsMode = false; // 벽에 맞아서 물리 엔진에 맡겨진 상태

    private IObjectPool<SpearProjectile> _myPool;
    private CancellationTokenSource _lifeTimeCts;

    private LevelData levelData;

    // 원래 레이어 저장용 (Reset할 때 필요)
    private int _defaultLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        levelData = InGameManager.Instance.currentLevelData;
        _defaultLayer = gameObject.layer; // 시작할 때 레이어 기억
    }

    public void SetPool(IObjectPool<SpearProjectile> pool)
    {
        _myPool = pool;
    }

    private void ResetState()
    {
        // 1. 부모 해제
        transform.SetParent(null);

        // 2. 물리 초기화 (가장 중요)
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = false;
        rb.gravityScale = 1f;

        // ★ [핵심] 물리 모드에서 다시 트리거 모드로 복구
        mouseCol.enabled = true;
        bodyCol.enabled = false;
        gameObject.layer = _defaultLayer; // 레이어 원상복구

        transform.rotation = Quaternion.identity;
        transform.localScale = Vector3.one * 0.65f;

        // 3. 플래그 초기화
        isStuck = false;
        isPhysicsMode = false;

        // 4. 트윈 및 효과 정리
        transform.DOKill();
        TrailRenderer trail = GetComponentInChildren<TrailRenderer>();
        if (trail != null) trail.Clear();
    }

    public void Launch(Vector3 startPos, Vector3 initialVelocity, float attackerGravity, float attackerFallMultiplier)
    {
        ResetState();

        transform.position = startPos;
        _fallMultiplier = attackerFallMultiplier;

        Vector2 finalVelocity = initialVelocity * levelData.projectile.speedMultiplier;
        rb.velocity = finalVelocity;

        float standardGravity = Physics2D.gravity.y;
        float gravityRatio = attackerGravity / standardGravity;
        _baseGravityScale = gravityRatio * (levelData.projectile.speedMultiplier * levelData.projectile.speedMultiplier);
        rb.gravityScale = _baseGravityScale;
    }

    private void FixedUpdate()
    {
        if (isStuck || isPhysicsMode) return;

        if (rb.velocity.y < 0) rb.gravityScale = _baseGravityScale * _fallMultiplier;
        else rb.gravityScale = _baseGravityScale;

        if (rb.velocity.sqrMagnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.velocity.y, rb.velocity.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }

    [SerializeField] private float wallBouncePower = 0.2f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isStuck || isPhysicsMode) return;

        int hitLayer = 1 << other.gameObject.layer;

        // 약점 레이어 체크 (최우선)
        if ((hitLayer & weakLayer) != 0)
        {
            // 약점 오브젝트 비활성화
            other.gameObject.SetActive(false);
            
            // Enemy Controller에 약점 맞춤 알림
            EnemyController enemyController = other.GetComponentInParent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.OnWeakPointHit();
            }
            
        }

        if ((hitLayer & enemyLayer) != 0)
        {
            StickToTarget(other, (hitLayer & enemyLayer) != 0);
        }
        else if ((hitLayer & groundLayer) != 0)
        {
            InGameManager.Instance.ResetCombo();
            StickToTarget(other, (hitLayer & enemyLayer) != 0);
        }
        else if ((hitLayer & blockLayer) != 0)
        {
            InGameManager.Instance.ResetCombo();
            EffectManager.Instance.PlayEffect(EffectType.Bird_Fail, transform.position, Vector3.one);
            EnablePhysicsBounce(other);
        }
        else if ((hitLayer & bounceLayer) != 0)
        {
            EffectManager.Instance.PlayEffect(EffectType.Hit_Hat, transform.position, Vector3.one);
            EnemyDamage.Instance?.OnHatHit(levelData.projectile.hatDamage);
            EnablePhysicsBounce(other);
        }
    }

    public void EnablePhysicsBounce(Collider2D wallCollider)
    {
        isPhysicsMode = true;

        animator.SetTrigger("Fail");

        bodyCol.enabled = true;
        mouseCol.enabled = false;
        rb.gravityScale = 3.0f;

        if (wallCollider == null)
        {
            rb.isKinematic = false;
            rb.velocity = new Vector2(rb.velocity.x * 0.3f, -2f);
            
            float randomTorque = Random.Range(300f, 700f) * (Random.value > 0.5f ? 1 : -1);
            rb.angularVelocity = randomTorque;
        }
        else
        {
            // 기존 로직: 벽에 튕김
            Vector2 incomingVelocity = rb.velocity;

            Vector2 hitPoint = wallCollider.ClosestPoint(transform.position);
            Vector2 surfaceNormal = ((Vector2)transform.position - hitPoint).normalized;

            Vector2 bounceVelocity = Vector2.Reflect(incomingVelocity, surfaceNormal);

            transform.position = hitPoint + (surfaceNormal * 0.1f);

            rb.velocity = bounceVelocity * wallBouncePower;

            float randomTorque = Random.Range(300f, 700f) * (Random.value > 0.5f ? 1 : -1);
            rb.angularVelocity = randomTorque;
        }

        HapticsManager.Instance.SoftImpactHaptic();

        LifetimeRoutine().Forget();
    }

    /// <summary>
    /// 게임 종료 시 화살을 새처럼 날려보내는 효과 (빵~!)
    /// </summary>
    public void ScatterLikeBird()
    {
        // 부모에서 분리
        transform.SetParent(null);
        
        // 물리 모드 활성화
        isPhysicsMode = true;
        isStuck = false;
        
        animator.SetTrigger("Fail");
        
        bodyCol.enabled = true;
        mouseCol.enabled = false;
        rb.isKinematic = false;
        rb.gravityScale = 2.5f;
        
        // 새들이 도망가는 느낌: 위쪽으로 강하게 + 랜덤 방향
        float randomAngleX = Random.Range(-1f, 1f); // 좌우 랜덤
        float upwardForce = Random.Range(8f, 12f);   // 위로 강하게
        float sidewaysForce = Random.Range(6f, 8f);  // 옆으로
        
        Vector2 scatterVelocity = new Vector2(
            randomAngleX * sidewaysForce,
            upwardForce
        );
        
        rb.velocity = scatterVelocity;
        
        // 회전도 랜덤하게
        float randomTorque = Random.Range(400f, 800f) * (Random.value > 0.5f ? 1 : -1);
        rb.angularVelocity = randomTorque;
        
        // 수명 루틴 시작
        LifetimeRoutine().Forget();
    }

    private void StickToTarget(Collider2D target, bool isEnemy)
    {
        if (isStuck) return;
        isStuck = true;

        StopPhysics(true);
        HapticsManager.Instance.SoftImpactHaptic();

        Vector3 wallSurfacePoint = target.ClosestPoint(transform.position);

        Vector3 backOffset = Vector3.zero;
        if (tipPoint != null)
        {
            backOffset = tipPoint.position - transform.position;
        }

        transform.position = wallSurfacePoint - backOffset;

        float currentAngle = transform.rotation.eulerAngles.z;
        float randomSpread = Random.Range(-10f, 10f);
        transform.rotation = Quaternion.Euler(0, 0, currentAngle + randomSpread);

        transform.SetParent(target.transform);

        if (isEnemy)
        {
            EnemyController enemyController = target.GetComponentInParent<EnemyController>();
            if (enemyController != null)
            {
                enemyController.RegisterStuckArrow(this);
            }
            EffectManager.Instance.PlayEffect(EffectType.Bird_Success, transform.position, Vector3.one );
            EnemyDamage.Instance?.OnDamaged(levelData.projectile.damage, target);
            animator.SetTrigger("Success");
            
        }
        else
        {
            EffectManager.Instance.PlayEffect(EffectType.Bird_Fail, transform.position, Vector3.one );
            animator.SetTrigger("Fail");
        }
    }

    private void StopPhysics(bool disableCollider)
    {
        rb.velocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.isKinematic = true;
        if (disableCollider)
        {
            mouseCol.enabled = false;
        }
    }

    private void OnDestroy()
    {
        transform.DOKill();
    }

    private void ReturnToPool()
    {
        if (!gameObject.activeSelf) return;
        if (_lifeTimeCts != null) { _lifeTimeCts.Cancel(); _lifeTimeCts.Dispose(); _lifeTimeCts = null; }

        if (_myPool != null) _myPool.Release(this);
        else Destroy(gameObject);
    }

    private async UniTaskVoid LifetimeRoutine()
    {
        if (_lifeTimeCts != null) _lifeTimeCts.Cancel();
        _lifeTimeCts = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(levelData.projectile.lifeTime), cancellationToken: _lifeTimeCts.Token);
            ReturnToPool();
        }
        catch (System.OperationCanceledException) { }
    }
}
