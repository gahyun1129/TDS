using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class EnemyDamage : MonoBehaviour
{
    public static EnemyDamage Instance { get; private set; }

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Transform headTransform; // ★ 머리 Transform
    [SerializeField] private GameObject spark;

    private SpriteRenderer[] _allRenderers;
    private Material[] _allMaterials;

    private Vector3 _originalScale;
    private Quaternion _originalHeadRotation; // ★ 머리 원래 회전값
    private CancellationTokenSource _cts;
    private CancellationTokenSource _headCts; // ★ 머리 애니메이션용 CTS
    private Sequence _currentSeq;
    private Sequence _headSeq; // ★ 머리 애니메이션용 Sequence

    [Header("Head Tilt Settings")]
    [SerializeField] private float headTiltAngle = -30f; // 머리 뒤로 젖혀지는 각도
    [SerializeField] private float headTiltDuration = 0.3f; // 젖혀지는 시간
    [SerializeField] private float headReturnDuration = 0.5f; // 돌아오는 시간

    LevelData levelData;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        _allRenderers = GetComponentsInChildren<SpriteRenderer>();
        _allMaterials = new Material[_allRenderers.Length];

        for (int i = 0; i < _allRenderers.Length; i++)
        {
            _allMaterials[i] = _allRenderers[i].material;
            _allMaterials[i].SetFloat("_FlashAmount", 0f);
        }
        _originalScale = transform.localScale;
        
        // 머리 원래 회전값 저장
        if (headTransform != null)
        {
            _originalHeadRotation = headTransform.localRotation;
        }
    }

    private void Start()
    {
        levelData = InGameManager.Instance.currentLevelData;
    }

    public void OnDamaged(float damage, Collider2D target)
    {
        PlayDamageEffect().Forget();

        if (target.CompareTag("Hand"))
        {
            damage *= 3;
            if (DamageTextPoolManager.Instance != null)
            {
                DamageTextPoolManager.Instance.ShowCriticalDamage(damage, target.transform.position);
            }
        }

        if (DamageTextPoolManager.Instance != null)
        {
            DamageTextPoolManager.Instance.ShowNormalDamage(damage, target.transform.position);
        }

        InGameManager.Instance.OnDamaged(damage);
    }

    private async UniTaskVoid PlayDamageEffect()
    {

        if (_cts != null) { _cts.Cancel(); _cts.Dispose(); }
        _cts = new CancellationTokenSource();

        if (_currentSeq != null && _currentSeq.IsActive()) _currentSeq.Kill();

        transform.localScale = _originalScale;

        foreach (var mat in _allMaterials)
        {
            if (mat != null) mat.SetFloat("_FlashAmount", 0f);
        }

        _currentSeq = DOTween.Sequence();

        _= _currentSeq.Join(transform.DOPunchScale(new Vector3(levelData.projectile.punchScaleStrength.x, levelData.projectile.punchScaleStrength.y, 0), levelData.projectile.punchDuration, 10, 1f));

        _= _currentSeq.Join(DOVirtual.Float(0f, 1f, levelData.projectile.flashDuration, (value) =>
        {
            // 이 람다 함수는 매 프레임 호출됨
            foreach (var mat in _allMaterials)
            {
                if (mat != null) mat.SetFloat("_FlashAmount", value);
            }
        }).SetLoops(2, LoopType.Yoyo)); // 0->1->0 깜빡

        try
        {
            await _currentSeq.ToUniTask(cancellationToken: _cts.Token);
        }
        catch (System.OperationCanceledException)
        {
        }
        finally
        {
            if (!_cts.IsCancellationRequested)
            {
                transform.localScale = _originalScale;
                foreach (var mat in _allMaterials)
                {
                    if (mat != null) mat.SetFloat("_FlashAmount", 0f);
                }
            }
        }
    }

    public void OnHatHit(float damage)
    {
        spark.SetActive(true);
        PlayHeadTiltBack().Forget();

        if (DamageTextPoolManager.Instance != null)
        {
            DamageTextPoolManager.Instance.ShowHatDamage(damage, headTransform.position);
        }

        InGameManager.Instance.OnDamaged(damage);
    }

    /// <summary>
    /// 머리를 뒤로 젖히는 애니메이션
    /// </summary>
    private async UniTaskVoid PlayHeadTiltBack()
    {
        if (headTransform == null || animator == null) return;

        // 기존 머리 애니메이션 취소
        if (_headCts != null) { _headCts.Cancel(); _headCts.Dispose(); }
        _headCts = new CancellationTokenSource();

        if (_headSeq != null && _headSeq.IsActive()) _headSeq.Kill();

        // 애니메이터 비활성화 (머리 애니메이션 중에는 Animator가 간섭하지 않도록)
        animator.enabled = false;

        // 머리를 원래 위치로 리셋
        headTransform.localRotation = _originalHeadRotation;

        _headSeq = DOTween.Sequence();

        // 1. 머리를 뒤로 젖히기 (Z축 회전)
        _= _headSeq.Append(headTransform.DOLocalRotate(
            new Vector3(0, 0, headTiltAngle), 
            headTiltDuration, 
            RotateMode.Fast)
            .SetEase(Ease.OutQuad));

        // 2. 원래 위치로 돌아오기
        _= _headSeq.Append(headTransform.DOLocalRotate(
            _originalHeadRotation.eulerAngles, 
            headReturnDuration, 
            RotateMode.Fast)
            .SetEase(Ease.OutElastic));

        try
        {
            await _headSeq.ToUniTask(cancellationToken: _headCts.Token);
        }
        catch (System.OperationCanceledException)
        {
        }
        finally
        {
            if (!_headCts.IsCancellationRequested && headTransform != null)
            {
                headTransform.localRotation = _originalHeadRotation;
            }

            // 애니메이터 다시 활성화
            if (animator != null)
            {
                spark.SetActive(false);
                animator.enabled = true;
            }
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _headCts?.Cancel();
        _headCts?.Dispose();
        
        _currentSeq?.Kill();
        _headSeq?.Kill();
        
        transform.DOKill();
        if (headTransform != null) headTransform.DOKill();
    }
}
