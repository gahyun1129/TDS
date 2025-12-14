// PopupBase.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 네임스페이스 추가

public enum PopupType
{
    None,
    GameFaile,
    GameSuccess,
    TimeStageInfo,
    CountStageInfo,
    WeakStageInfo,
    TodayReward,
    Preview,
    StartInfo,
    // ...
}

// 이 팝업은 독립적인 Canvas와 Raycaster를 가집니다.
[RequireComponent(typeof(Canvas))]
[RequireComponent(typeof(CanvasGroup))]
[RequireComponent(typeof(GraphicRaycaster))]
public abstract class PopupBase : MonoBehaviour
{
    protected Canvas popupCanvas;
    protected CanvasGroup canvasGroup;
    protected PopupManager popupManager;

    // --- 애니메이션 관련 설정 ---
    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private float slideDistance = 1000f; // 슬라이드 거리
    [SerializeField] private Ease showEase = Ease.OutBack; // 열릴 때 탄성 효과
    [SerializeField] private Ease hideEase = Ease.InOutQuad; // 닫힐 때 부드러운 효과

    protected bool isAnimating = false;
    private Vector3 originalPosition;
    private Sequence currentSequence; // 현재 실행 중인 DOTween Sequence 추적

    protected virtual void Awake()
    {
        popupCanvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true; // 항상 레이캐스트 차단 (애니메이션 중에도 터치 막기)
        
        // 원래 위치 저장
        originalPosition = transform.localPosition;
        
        gameObject.SetActive(false);
    }

    public virtual void SetManager(PopupManager manager)
    {
        popupManager = manager;
    }

    /// <summary>
    /// 매니저가 이 팝업의 Sorting Order를 설정해줍니다.
    /// </summary>
    public virtual void SetSortingOrder(int order)
    {
        popupCanvas.overrideSorting = true;
        popupCanvas.sortingOrder = order;
    }

    /// <summary>
    /// 팝업을 엽니다. (DOTween 애니메이션 포함 - 아래에서 위로 슈웅 틱!)
    /// </summary>
    public virtual void Open()
    {
        if (isAnimating || gameObject.activeSelf) return;
        isAnimating = true;

        // 기존 애니메이션 정리
        KillCurrentSequence();

        gameObject.SetActive(true);
        
        // 팝업이 열리는 순간부터 터치 차단 (애니메이션 중에도)
        canvasGroup.blocksRaycasts = true;

        // 시작 위치를 아래로 설정
        transform.localPosition = originalPosition + Vector3.down * slideDistance;
        canvasGroup.alpha = 0;

        currentSequence = DOTween.Sequence();
        
        // 페이드 인
        currentSequence.Join(canvasGroup.DOFade(1, animationDuration * 0.3f));
        
        // 아래에서 위로 슬라이드 (OutBack으로 탄성있는 "틱" 효과)
        currentSequence.Join(transform.DOLocalMove(originalPosition, animationDuration).SetEase(showEase));

        currentSequence.OnComplete(() =>
        {
            canvasGroup.interactable = true;
            isAnimating = false;
            currentSequence = null;
            OnOpened();
        });
    }

    /// <summary>
    /// 팝업을 닫습니다. (DOTween 애니메이션 포함 - 위에서 아래로 슈웅~~)
    /// </summary>
    public virtual void Close()
    {
        if (isAnimating || !gameObject.activeSelf) return;
        isAnimating = true;

        // 기존 애니메이션 정리
        KillCurrentSequence();

        canvasGroup.interactable = false;
        // 닫히는 동안에도 터치 차단 유지
        canvasGroup.blocksRaycasts = true;

        currentSequence = DOTween.Sequence();
        
        // 페이드 아웃
        currentSequence.Join(canvasGroup.DOFade(0, animationDuration));
        
        // 위에서 아래로 부드럽게 슬라이드
        currentSequence.Join(transform.DOLocalMove(originalPosition + Vector3.down * slideDistance, animationDuration).SetEase(hideEase));

        currentSequence.OnComplete(() =>
        {
            gameObject.SetActive(false);
            isAnimating = false;
            currentSequence = null;

            if (popupManager != null)
            {
                popupManager.NotifyPopupClosed(this);
            }

            OnClosed();
        });
    }

    /// <summary>
    /// 현재 실행 중인 DOTween Sequence를 안전하게 종료
    /// </summary>
    private void KillCurrentSequence()
    {
        if (currentSequence != null && currentSequence.IsActive())
        {
            currentSequence.Kill();
            currentSequence = null;
        }
    }

    protected virtual void OnDisable()
    {
        KillCurrentSequence();
        transform.DOKill();
        canvasGroup.DOKill();
    }

    protected virtual void OnDestroy()
    {
        KillCurrentSequence();
        transform.DOKill();
        canvasGroup.DOKill();
    }

    public virtual int GetSortingOrder()
    {
        return popupCanvas.sortingOrder;
    }

    protected virtual void ClearPopUP() { }
    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}
