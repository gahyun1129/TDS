// PopupBase.cs
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening; // DOTween 네임스페이스 추가

public enum PopupType
{
    None,
    Settings,
    Confirm,
    Notice,
    Shop,
    Inventory,
    Stat,
    Skill,
    Rune
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
    [SerializeField] private float fadeDuration = 0.2f;
    [SerializeField] private Ease showEase = Ease.OutBack;
    [SerializeField] private Ease hideEase = Ease.OutSine;
    [SerializeField] private float initialScale = 0.8f;

    protected bool isAnimating = false;

    protected virtual void Awake()
    {
        popupCanvas = GetComponent<Canvas>();
        canvasGroup = GetComponent<CanvasGroup>();

        // 시작 시 비활성화 및 초기 상태 설정
        canvasGroup.alpha = 0;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
        transform.localScale = Vector3.one * initialScale;
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
    /// 팝업을 엽니다. (DOTween 애니메이션 포함)
    /// </summary>
    public virtual void Open()
    {
        if (isAnimating || gameObject.activeSelf) return;
        isAnimating = true;

        gameObject.SetActive(true);

        Sequence seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(1, fadeDuration));
        
        transform.localScale = Vector3.one * initialScale;
        seq.Join(transform.DOScale(1, fadeDuration).SetEase(showEase));

        seq.OnComplete(() =>
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            isAnimating = false;
            OnOpened();
        });
    }

    /// <summary>
    /// 팝업을 닫습니다. (DOTween 애니메이션 포함)
    /// </summary>
    public virtual void Close()
    {
        if (isAnimating || !gameObject.activeSelf) return;
        isAnimating = true;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        Sequence seq = DOTween.Sequence();
        seq.Join(canvasGroup.DOFade(0, fadeDuration));
        seq.Join(transform.DOScale(initialScale, fadeDuration).SetEase(hideEase));

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
            isAnimating = false;
            
            // (중요) 애니메이션이 끝난 후 매니저에게 닫혔음을 알림
            if (popupManager != null)
            {
                popupManager.NotifyPopupClosed(this);
            }
            
            OnClosed();
        });
    }

    protected virtual void OnOpened() { }
    protected virtual void OnClosed() { }
}