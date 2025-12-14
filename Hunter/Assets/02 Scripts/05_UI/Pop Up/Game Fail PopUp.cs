using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Cysharp.Threading.Tasks;

public class GameFailePopUp : PopupBase
{
    [SerializeField] private TextMeshProUGUI stage_text;
    [SerializeField] Animator animator;

    [SerializeField] private RectTransform starUI;

    public void SetUp()
    {
        animator.Play("Show");
        stage_text.text = $"Day {GameDataManager.Instance.currentLevelData.stageNum}";
    }

    public void OnClickedRetryButton()
    {
        FadeOutToTarget(targetUI, "InGame").Forget();
    }
    public void OnClickedHomeButton()
    {
        FadeOutToTarget(targetUI, "Title").Forget();
    }

    [Header("설정")]
    private Material transitionMat; 
    [SerializeField] private RectTransform targetUI;
    [SerializeField] private Canvas parentCanvas;  

    [Header("애니메이션 튜닝")]
    [SerializeField] private float maxRadius = 1.5f;

    [SerializeField] private float targetRadius = 0.15f;
    
    [Tooltip("등장 시 순간적으로 커질 최대 크기 배율")]
    [SerializeField] private float peakScale = 1.4f; 

    [Tooltip("시작할 때의 회전 각도 (Z축)")]
    [SerializeField] private float startAngle = -90f;
    public Image transitionImage;
    private Sequence glintSequence;

    void Start()
    {
        transitionMat = transitionImage.material;
        transitionMat.SetFloat("_Radius", maxRadius);
        transitionImage.enabled = true;
    }

    [ContextMenu("PlayCloseAnimation")]
    public void PlayCloseAnimation()
    {
        FadeOutToTarget(targetUI, "InGame").Forget();
    }

    public async UniTaskVoid FadeOutToTarget(RectTransform target, string nextScene)
    {
        if (target == null || transitionMat == null) return;

        transitionImage.enabled = true;

        transitionMat.SetVector("_Center", new Vector4(0.48f, 0.65f, 0, 0));

        transitionMat.SetFloat("_Radius", maxRadius);

        await transitionMat.DOFloat(targetRadius, "_Radius", 1f)
            .SetEase(Ease.InQuad)
            .SetUpdate(true) 
            .ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        PlayGlintEffect();

        await UniTask.Delay(1000, ignoreTimeScale: true, cancellationToken: this.GetCancellationTokenOnDestroy());
        animator.enabled = true;
        Sequence finalSeq = DOTween.Sequence();
        _= finalSeq.SetUpdate(true); 

        _= finalSeq.Append(transitionMat.DOFloat(targetRadius * 1f, "_Radius", 0.2f));

        _= finalSeq.Append(transitionMat.DOFloat(-0.5f, "_Radius", 1f).SetEase(Ease.OutBack));

        await finalSeq.ToUniTask(cancellationToken: this.GetCancellationTokenOnDestroy());

        Close();
        SceneChanger.Instance.LoadSceneWithFadeIn(nextScene);
    }

    public void FadeIn()
    {
        transitionImage.enabled = true;
        transitionMat.SetFloat("_Radius", 0f);
        transitionMat.DOFloat(maxRadius, "_Radius", 0.8f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() => transitionImage.enabled = false);
    }

    public void PlayGlintEffect()
    {
        if (glintSequence != null && glintSequence.IsActive())
        {
            glintSequence.Kill();
        }

        animator.enabled = false;

        starUI.localScale = Vector3.zero;
        starUI.localRotation = Quaternion.Euler(0, 0, startAngle);

        glintSequence = DOTween.Sequence();

        float phase1Dur = 1f * 0.4f; 
        float phase2Dur = 1f * 0.6f; 

        glintSequence.Append(starUI.DOScale(Vector3.one * peakScale, phase1Dur).SetEase(Ease.OutCirc));
        glintSequence.Join(starUI.DORotate(new Vector3(0, 0, 30f), phase1Dur).SetEase(Ease.OutCirc));

        glintSequence.Append(starUI.DOScale(Vector3.one, phase2Dur).SetEase(Ease.InOutQuad));
        glintSequence.Join(starUI.DORotate(Vector3.zero, phase2Dur).SetEase(Ease.InOutQuad));
        
        // (선택사항) 효과음 타이밍 맞추기
        // 0.1초 지점에 "띵~" 소리 재생 함수 연결
        // glintSequence.InsertCallback(0.1f, () => SoundManager.PlayOneShot("DingSound"));
    }
}
