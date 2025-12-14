using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class ShowInGameUI : MonoBehaviour
{
    [SerializeField] Image inBirdIcon, inEnemyIcon;
    [SerializeField] Transform inBirdCount, inHPBar, inHPvalue;
    [SerializeField] Transform inPause;

    public void Hide()
    {
        inBirdIcon.transform.localScale = Vector2.zero;
        inBirdCount.localScale = Vector2.zero;

        inEnemyIcon.transform.localScale = Vector2.zero;
        inHPBar.localScale = Vector2.zero;
        inHPvalue.localScale = Vector2.zero;
        inPause.localScale = Vector2.zero;
    }

    public async UniTaskVoid Show()
    {
        await UniTask.Delay(1000, cancellationToken: this.GetCancellationTokenOnDestroy());

        _= inBirdIcon.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
        _= inBirdCount.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f).SetLink(gameObject);

        _= inEnemyIcon.transform.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
        _= inHPBar.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.1f).SetLink(gameObject);
        _= inHPvalue.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.2f).SetLink(gameObject);
        _= inPause.DOScale(1f, 0.3f).SetEase(Ease.OutBack).SetDelay(0.3f).SetLink(gameObject);
    }
}
