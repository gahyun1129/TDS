using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class UITest : MonoBehaviour
{
    [SerializeField] Button playButton;
    [SerializeField] Animator animator;

    [SerializeField] TextMeshProUGUI missionText;
    [SerializeField] Image missionIcon;
    [SerializeField] TextMeshProUGUI dayText;

    [SerializeField] Sprite[] icons = new Sprite[2];

    [SerializeField] Image inBirdIcon;

    [SerializeField] ShowInGameUI ingameUI;
    void Start()
    {
        playButton.onClick.AddListener(GetBtnPlay);
        SetUp();
        ingameUI.Hide();
    }

    public void SetUp()
    {
        LevelData levelData;

        if (InGameManager.Instance != null)
        {
            levelData = InGameManager.Instance.currentLevelData;
            dayText.text = $"Day {levelData.stageNum}";
            switch (levelData.mission.stageType)
            {
                case StageType.time_limit:
                    {
                        missionIcon.sprite = icons[0];
                        inBirdIcon.sprite = icons[0];
                        birdIcon.sprite = icons[0];
                        missionText.text = $"{levelData.mission.timeLimit:F0}";
                        break;
                    }
                case StageType.count_limit:
                    {
                        missionIcon.sprite = icons[1];
                        inBirdIcon.sprite = icons[1];
                        birdIcon.sprite = icons[1];
                        missionText.text = $"x{levelData.mission.countLimit:F0}";
                        break;
                    }
                case StageType.weak_point:
                    {
                        missionIcon.sprite = icons[1];
                        inBirdIcon.sprite = icons[1];
                        birdIcon.sprite = icons[1];
                        missionText.text = $"x{levelData.mission.countLimit:F0}";
                        break;
                    }
            }            
        }
    }
    
    public void GetBtnPlay()
    {
        if ( InGameManager.Instance != null)
        {
            InGameManager.Instance.GameStart();
        }
        animator.Play("Hide");
    }


    [SerializeField] Transform birdStartTrans, enemyStartTrnas;
    [SerializeField] Transform birdTarget, enemyTarget;
    [SerializeField] Image birdIcon, enemyIcon;
    [SerializeField] float iconScale = 1.5f;
    [SerializeField] float moveDuation = 0.5f;
    [SerializeField] float sclaeUpDuration = 0.1f;
    [SerializeField] float scaleEndDuration = 0.5f;
    [SerializeField] float moveDelay = 0.2f;
    [SerializeField] float scaleDelay = 0.2f;
    
    public void MoveIcon()
    {
        birdIcon.gameObject.SetActive(true);
        enemyIcon.gameObject.SetActive(true);

        birdIcon.transform.position = birdStartTrans.position;
        enemyStartTrnas.position = enemyStartTrnas.position;

        birdIcon.DOKill();
        enemyIcon.DOKill();

        birdIcon.transform.DOScale(iconScale, sclaeUpDuration).SetLink(gameObject);;
        enemyIcon.transform.DOScale(iconScale, sclaeUpDuration).SetLink(gameObject);;

        birdIcon.transform.DOMove(birdTarget.position, moveDuation).SetDelay(moveDelay).SetLink(gameObject);;
        enemyIcon.transform.DOMove(enemyTarget.position, moveDuation).SetDelay(moveDelay).SetLink(gameObject);;

        scaleDelay += sclaeUpDuration;

        birdIcon.transform.DOScale(0f, scaleEndDuration).SetDelay(scaleDelay).OnComplete(()=>birdIcon.gameObject.SetActive(false)).SetLink(gameObject);;
        enemyIcon.transform.DOScale(0f, scaleEndDuration).SetDelay(scaleDelay).OnComplete(() => enemyIcon.gameObject.SetActive(false)).SetLink(gameObject);;

        ingameUI.Show().Forget();
    }
}
