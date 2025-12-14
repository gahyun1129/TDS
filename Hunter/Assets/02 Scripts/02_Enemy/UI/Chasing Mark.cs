using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;
using System.Threading;

public class ChasingMark : MonoBehaviour
{
    [Header("Punch Animation Settings")]
    [SerializeField] private float punchScale = 0.3f; // 펀치 스케일 강도
    [SerializeField] private float punchDuration = 0.3f; // 펀치 애니메이션 시간
    [SerializeField] private float deactivateDelay = 1f; // 비활성화까지 대기 시간

    private Vector3 _originalScale;
    private CancellationTokenSource _cts;
    private bool _isAnimating = false;

    private void Awake()
    {
        _originalScale = transform.localScale;
    }

    private void OnEnable()
    {
        // 스케일 초기화
        if (transform != null)
        {
            transform.localScale = _originalScale;
        }

        // 깜짝 놀란 듯한 펀치 애니메이션 + 1초 후 비활성화
        PlayPunchAndDeactivate().Forget();
    }

    private async UniTaskVoid PlayPunchAndDeactivate()
    {
        // 기존 작업 취소
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();

        _isAnimating = true;

        try
        {
            // 깜짝 놀란 듯한 펀치 효과
            await transform.DOPunchScale(Vector3.one * punchScale, punchDuration, 10, 1f)
                .SetEase(Ease.OutElastic)
                .SetLink(gameObject) // UI 오브젝트와 연결
                .ToUniTask(cancellationToken: _cts.Token);

            // 1초 대기
            await UniTask.Delay((int)(deactivateDelay * 1000), cancellationToken: _cts.Token);

            // 비활성화
            if (this != null && gameObject != null && gameObject.activeInHierarchy)
            {
                gameObject.SetActive(false);
            }
        }
        catch (System.OperationCanceledException)
        {
            // 취소됨 - 정상 동작
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"ChasingMark animation error: {ex.Message}");
        }
        finally
        {
            _isAnimating = false;
            
            // 스케일 복원
            if (this != null && transform != null)
            {
                transform.localScale = _originalScale;
            }
        }
    }

    private void OnDisable()
    {
        // 애니메이션 중이 아닐 때만 정리
        if (!_isAnimating)
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            // DOTween 정리
            if (transform != null)
            {
                transform.DOKill();
                transform.localScale = _originalScale;
            }
        }
    }

    private void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        
        if (transform != null)
        {
            transform.DOKill();
        }
    }
}
