using UnityEngine;
using UnityEngine.Pool;
using TMPro;
using DG.Tweening;
using System.Threading;

/// <summary>
/// 데미지 텍스트 기본 클래스
/// </summary>
public class DamageTextBase : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] protected TextMeshProUGUI damageText;
    [SerializeField] private Animator animator;
    [SerializeField] private float rot = 5;

    protected IObjectPool<DamageTextBase> _pool;
    protected CancellationTokenSource _cts;
    protected Vector3 _originalScale;

    protected virtual void Awake()
    {
        if (damageText == null)
        {
            damageText = GetComponent<TextMeshProUGUI>();
        }
        _originalScale = transform.localScale;
    }

    /// <summary>
    /// 풀 설정
    /// </summary>
    public void SetPool(IObjectPool<DamageTextBase> pool)
    {
        _pool = pool;
    }

    /// <summary>
    /// 데미지 텍스트 초기화 및 애니메이션 시작
    /// </summary>
    public virtual void Initialize(float damage, Vector3 worldPosition, bool randomRotation = false)
    {
        // 월드 포지션을 UI 스크린 포지션으로 변환
        Vector3 screenPosition = Camera.main.WorldToScreenPoint(worldPosition);
        
        // RectTransform 사용 (UI 요소이므로)
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            // 스크린 포지션을 Canvas의 로컬 포지션으로 변환
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvas.transform as RectTransform,
                    screenPosition,
                    canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : Camera.main,
                    out Vector2 localPoint
                );
                rectTransform.localPosition = localPoint;
            }
            else
            {
                // Canvas를 찾지 못한 경우 스크린 포지션 직접 사용
                rectTransform.position = screenPosition;
            }

            // 랜덤 회전 적용 (대각선 정도: -45도 ~ 45도)
            if (randomRotation)
            {
                float randomAngle = Random.Range(-rot, rot);
                rectTransform.localRotation = Quaternion.Euler(0, 0, randomAngle);
            }
            else
            {
                rectTransform.localRotation = Quaternion.identity;
            }
        }
        else
        {
            // RectTransform이 아닌 경우 (일반 Transform)
            transform.position = screenPosition;
            
            if (randomRotation)
            {
                float randomAngle = Random.Range(-rot, rot);
                transform.localRotation = Quaternion.Euler(0, 0, randomAngle);
            }
            else
            {
                transform.localRotation = Quaternion.identity;
            }
        }

        // 텍스트 설정
        damageText.text = Mathf.RoundToInt(damage).ToString();

        animator.Play("Show");
    }

    /// <summary>
    /// 위치 변경 없이 회전만 랜덤으로 적용 (콤보용)
    /// </summary>
    public virtual void Initialize(float damage)
    {
        // RectTransform 사용 (UI 요소이므로)
        RectTransform rectTransform = transform as RectTransform;
        if (rectTransform != null)
        {
            // 랜덤 회전 적용 (대각선 정도: -45도 ~ 45도)
            float randomAngle = Random.Range(-rot, rot);
            rectTransform.localRotation = Quaternion.Euler(0, 0, randomAngle);
        }
        else
        {
            // RectTransform이 아닌 경우 (일반 Transform)
            float randomAngle = Random.Range(-rot, rot);
            transform.localRotation = Quaternion.Euler(0, 0, randomAngle);
        }

        // 텍스트 설정
        damageText.text = Mathf.RoundToInt(damage).ToString();

        animator.Play("Show");
    }

    /// <summary>
    /// 풀로 반환
    /// </summary>
    public void ReturnToPool()
    {
        if (_pool != null)
        {
            _pool.Release(this);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    protected virtual void OnDisable()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;

        transform.DOKill();
    }

    protected virtual void OnDestroy()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = null;
        
        transform.DOKill();
    }
}
