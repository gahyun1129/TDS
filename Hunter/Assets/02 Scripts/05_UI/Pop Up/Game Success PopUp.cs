using TMPro;
using UnityEngine;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;

public class GameSuccessPopUp : PopupBase
{
    [Header("결과 정보 담을 오브젝트")]
    [SerializeField] private TextMeshProUGUI dayInfoText;

    [Header("보상 정보 담을 오브젝트")]
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Transform iconSpawnCanvas;
    [SerializeField] private CurrencyUI iconTargetTransform;
    
    [SerializeField] Animator animator;

    [Header("재화 프리팹")]
    [SerializeField] private GameObject currencyObj;

    public void SetUp()
    {
        iconTargetTransform.ShowUI();
        dayInfoText.text = $"Day {GameDataManager.Instance.currentLevelData.stageNum}";
        animator.Play("Show");
    }

    public void OnClickedNextStageButton()
    {
        GameDataManager.Instance.GoToNextStage();
        SceneChanger.Instance.LoadSceneWithFade("InGame");
        Close();
    }

    public void OnClickedHomeButton()
    {
        GameDataManager.Instance.GoToNextStage();
        SceneChanger.Instance.LoadSceneWithFade("Title");
        Close();
    }

    [ContextMenu("Play Effect")]
    public void PlayEffect()
    {
        PlayGetItemEffect(iconSpawnCanvas, rewardText.transform.position, iconTargetTransform.transform, 10).Forget();
    }

    public async UniTask PlayGetItemEffect(Transform effectCanvasTransform, Vector3 startWorldPos, Transform targetTransform, int count = 10)
    {
        await FlyIconsProcess(effectCanvasTransform, startWorldPos, targetTransform, count);
    }

    private async UniTask FlyIconsProcess(Transform effectCanvasTransform, Vector3 startPos, Transform target, int count)
    {
        List<GameObject> icons = new List<GameObject>();
        float spawnRadius = 100f;

        for (int i = 0; i < count; i++)
        {
            GameObject icon = Instantiate(currencyObj, effectCanvasTransform);
            icon.transform.position = startPos;
            icons.Add(icon);

            float angle = Random.Range(0f, 360f);
            float dist = Random.Range(spawnRadius * 0.5f, spawnRadius);
            Vector3 scatterPos = startPos + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * dist;

            _ = icon.transform.DOMove(scatterPos, 0.3f).SetEase(Ease.OutBack).SetLink(gameObject);
            icon.transform.localScale = Vector3.one * Random.Range(0.8f, 1.2f);
        }

        await UniTask.Delay(100, cancellationToken: this.GetCancellationTokenOnDestroy());

        foreach (var icon in icons)
        {
            if (icon == null) continue;

            Vector3 targetPos = (target != null) ? target.position : startPos;
            float delay = Random.Range(0f, 0.2f);

            FlyToTarget(icon, target, delay).Forget();
        }
    }

    private async UniTaskVoid FlyToTarget(GameObject icon, Transform target, float delay)
    {
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: this.GetCancellationTokenOnDestroy());
        if (icon == null) return;

        
        var move = icon.transform.DOMove(target.position, 1f).SetEase(Ease.InBack).SetLink(gameObject).ToUniTask();
        var scale = icon.transform.DOScale(0.5f, 1f).SetLink(gameObject).ToUniTask();

        await UniTask.WhenAll(move, scale)
            .AttachExternalCancellation(this.GetCancellationTokenOnDestroy());

        iconTargetTransform.GetEffect();

        Destroy(icon);

        await UniTask.Delay(3000, cancellationToken: this.GetCancellationTokenOnDestroy());

        iconTargetTransform.HideUI();
    }

}

