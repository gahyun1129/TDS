using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    #region singleton
    public static UIManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    #endregion

    [SerializeField] private TextMeshProUGUI stage_text;
    [SerializeField] private TextMeshProUGUI health_text;
    [SerializeField] private Slider health_slider;
    [SerializeField] private GameObject warning;
    [SerializeField] private ShowInGameUI inGameUI;

    [Header("애니메이션 설정")]
    [SerializeField] private float AnimationDuration = 0.5f;
    [SerializeField] private Ease AnimationEase = Ease.OutQuad;

    LevelData levelData;

    public void GameStart()
    {
        levelData = InGameManager.Instance.currentLevelData;
        InGameManager.Instance.OnGameEnd.AddListener(OnGameEnd);

        switch (levelData.mission.stageType)
        {
            case StageType.time_limit:
                {
                    stage_text.text = $"{levelData.mission.timeLimit:F0}";

                    break;
                }
            case StageType.count_limit:
                {
                    stage_text.text = $"X{levelData.mission.countLimit:F0}";
                    break;
                }
            case StageType.weak_point:
                {
                    stage_text.text = $"X{levelData.mission.countLimit:F0}";
                    break;
                }
        }

        UpdateHP(levelData.enemyBase.maxHealth, levelData.enemyBase.maxHealth);
    }

    public void OnGameEnd(bool isWin)
    {
        inGameUI.Hide();
    }

    public void UpdateHP(float health, float maxHealth)
    {
        float targetValue = health / maxHealth;

        health_slider.DOValue(targetValue, AnimationDuration)
            .SetEase(AnimationEase);

        health_text.text = $"{health:F0}/{maxHealth:F0}";

    }

    public void UpdateTimeUI(int seconds)
    {
        if (levelData.mission.stageType == StageType.time_limit)
        {
            stage_text.text = $"{seconds}";

            if ( seconds == 10)
            {
                StartWarning().Forget();
            }
        }
    }

    public void UpdateArrowsCount(int count)
    {
        stage_text.text = $"X{count}";
    }

    public void OnClickedPauseButton()
    {
        InGameManager.Instance.Pause();
    }

    public void OnClickedResumButton()
    {
        InGameManager.Instance.Resume();
    }

    public void OnClickedHomeButton()
    {
        SceneChanger.Instance.LoadSceneWithFade("Title");
    }
    
    [ContextMenu("Test Warning Blink")]
    public async UniTaskVoid StartWarning()
    {
        warning.SetActive(true);
        
        // CanvasGroup 컴포넌트 가져오기 (없으면 추가)
        CanvasGroup canvasGroup = warning.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = warning.AddComponent<CanvasGroup>();
        }
        
        canvasGroup.alpha = 1f;
        Sequence blinkSequence = DOTween.Sequence();
        _ = blinkSequence.Append(canvasGroup.DOFade(0f, 0.3f))
                     .Append(canvasGroup.DOFade(1f, 0.3f))
                     .Append(canvasGroup.DOFade(0f, 0.3f))
                     .Append(canvasGroup.DOFade(1f, 0.3f))
                     .Append(canvasGroup.DOFade(0f, 0.3f))
                     .Append(canvasGroup.DOFade(1f, 0.3f))
                     .Append(canvasGroup.DOFade(0f, 0.3f));
        
        await blinkSequence.AsyncWaitForCompletion();
        warning.SetActive(false);
    }    
}
