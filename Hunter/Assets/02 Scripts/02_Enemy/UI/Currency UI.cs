using DG.Tweening;
using TMPro;
using UnityEngine;

public class CurrencyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI currencyText;
    [SerializeField] private RectTransform currencyTransform;
    
    [SerializeField] private float startYOffset = 500f; // 얼마나 위에서 떨어질지
    [SerializeField] private float duration = 1f; // 떨어지는 데 걸리는 시간

    private Vector2 originalPos;

    void Awake()
    {
        originalPos = currencyTransform.anchoredPosition;
    }

    void OnEnable()
    {
        ShowUI();
    }

    void OnDisable()
    {
        transform.DOKill();
        currencyTransform.DOKill();
        currencyText.DOKill();
    }

    public void GetEffect()
    {
        Tween scaleTween = transform
            .DOScale(1.2f, 0.1f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject);
            
        int reward = 20;
        int currentCurrency = int.Parse(currencyText.text);
        DOVirtual.Int(currentCurrency, currentCurrency + reward, duration, (v) =>
        {
            currencyText.text = v.ToString();
        })
        .SetEase(Ease.OutExpo)
        .OnComplete(() =>
        {
            scaleTween.Kill();
            transform.localScale = Vector3.one;
        })
        .SetLink(gameObject);

        // HideUI();
    }

    [ContextMenu("ShowUI")]
    public void ShowUI()
    {
        currencyTransform.anchoredPosition = new Vector2(originalPos.x, originalPos.y + startYOffset);

        currencyTransform.DOAnchorPos(originalPos, duration)
               .SetEase(Ease.OutBounce)
               .SetLink(gameObject);
    }
    
    [ContextMenu("HideUI")]
    public void HideUI()
    {
        currencyTransform.anchoredPosition = originalPos;
        Vector2 targetTransform = new Vector2(originalPos.x, originalPos.y + startYOffset);

        currencyTransform.DOAnchorPos(targetTransform, duration)
               .SetEase(Ease.InBounce)
               .SetLink(gameObject);
    }
}
