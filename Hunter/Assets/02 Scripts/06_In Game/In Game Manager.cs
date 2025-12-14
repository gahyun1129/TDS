using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Solo.MOST_IN_ONE;
using UnityEngine;
using UnityEngine.Events;

public class InGameManager : MonoBehaviour
{
    #region singleton
    public static InGameManager Instance { get; private set; }
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }        
    }
    #endregion

    #region parameter

    // data
    public LevelData    currentLevelData;
    public float        currentEnemyHealth { get; private set; } = 0f;
    public float        currentTime { get; private set; } = 0f;
    public int          currentArrowCount { get; private set; } = 0;

    // run-time update
    public bool         isGameActive { get; private set; } = false;
    private float timeAccumulator = 0f;
    public bool canAttack = false;
    private int comboCount = 0;

    public UnityEvent<bool> OnGameEnd = new UnityEvent<bool>();
    public UnityEvent OnGameStart = new UnityEvent();
    public UnityEvent<bool> OnGamePause = new UnityEvent<bool>();

    [SerializeField] private bool isCustom = false;

    #endregion

    public void Start()
    {
        if (GameDataManager.Instance != null)
        {
            currentLevelData = GameDataManager.Instance.currentLevelData;
        }
        
        if (isCustom)
        {
            switch (currentLevelData.mission.stageType)
            {
                case StageType.time_limit:
                    {
                        PopupManager.Instance.ShowPopup(PopupType.TimeStageInfo);
                        break;
                    }
                case StageType.count_limit:
                    {
                        PopupManager.Instance.ShowPopup(PopupType.CountStageInfo);
                        break;
                    }
                case StageType.weak_point:
                    {
                        PopupManager.Instance.ShowPopup(PopupType.WeakStageInfo);
                        break;
                    }
            }
        }
        else
        {
            if (currentLevelData.mission.stageType == StageType.weak_point)
            {
                EnemyController enemyController = FindObjectOfType<EnemyController>();
                if (enemyController != null)
                {
                    int totalAvailablePoints = 8;

                    int[] randomIndices = GenerateRandomIndices(totalAvailablePoints, currentLevelData.mission.weakPointCount);

                    enemyController.SetActiveWeakPoints(randomIndices);
                }
            }

            // GameStart();
        }
    }

    private int[] GenerateRandomIndices(int maxRange, int count)
    {
        List<int> allIndices = Enumerable.Range(0, maxRange).ToList();
        
        for (int i = 0; i < allIndices.Count; i++)
        {
            int temp = allIndices[i];
            int randomIndex = Random.Range(i, allIndices.Count);
            allIndices[i] = allIndices[randomIndex];
            allIndices[randomIndex] = temp;
        }

        return allIndices.Take(count).ToArray();
    }

    public void GameStart()
    {
        currentEnemyHealth = currentLevelData.enemyBase.maxHealth;

        if (currentLevelData.mission.stageType == StageType.time_limit)
        {
            currentTime = currentLevelData.mission.timeLimit;
        }
        else if (currentLevelData.mission.stageType == StageType.count_limit || currentLevelData.mission.stageType == StageType.weak_point)
        {
            currentArrowCount = currentLevelData.mission.countLimit;
        }

        UIManager.Instance.GameStart();

        isGameActive = true;
        OnGameStart?.Invoke();
    }

    void Update()
    {
        if (isGameActive && currentLevelData.mission.stageType == StageType.time_limit)
        {
            timeAccumulator += Time.deltaTime;
            if (timeAccumulator >= 1f)
            {
                timeAccumulator -= 1f;
                currentTime--;

                if (currentTime < 0) currentTime = 0;

                UIManager.Instance.UpdateTimeUI((int)currentTime);

                if (currentTime <= 0)
                {
                    GameEnd(false).Forget();
                }
            }
        }
    }

    public bool TryThrow()
    {
        if (currentLevelData.mission.stageType == StageType.time_limit) return true;

        currentArrowCount--;
        UIManager.Instance.UpdateArrowsCount(currentArrowCount);
        if (currentArrowCount <= 0)
        {

            return false;
        }
        return true;
    }

    public void OnPlyerHit()
    {
        if (canAttack)
        {
            GameEnd(false).Forget();
        }
    }

    public void OnDamaged(float damage)
    {
        comboCount++;

        if (comboCount >= 2)
        {
            DamageTextPoolManager.Instance.ShowCombo(comboCount);
            damage += comboCount * 10;
        }

        currentEnemyHealth -= damage;

        if (currentEnemyHealth <= 0)
        {
            currentEnemyHealth = 0;

            if (currentLevelData.mission.stageType == StageType.weak_point)
            {
                EnemyController enemyController = FindObjectOfType<EnemyController>();
                if (enemyController != null && enemyController.GetRemainingWeakPoints() > 0)
                {
                    UIManager.Instance.UpdateHP(currentEnemyHealth, currentLevelData.enemyBase.maxHealth);
                    return;
                }
            }

            GameEnd(true).Forget();
        }

        UIManager.Instance.UpdateHP(currentEnemyHealth, currentLevelData.enemyBase.maxHealth);
    }
    
    public void ResetCombo()
    {
        comboCount = 0;
    }

    public async UniTask GameEnd(bool isWin)
    {
        isGameActive = false;

        OnGameEnd?.Invoke(isWin);
        HapticsManager.Instance.MediumImpactHaptic();

        await UniTask.Delay(5000);

        if (isWin)
        {
            GameClear().Forget();
            return;
        }
        GameOver();
    }

    void GameOver()
    {
        PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.GameFaile);
        GameFailePopUp failPopUp = popup as GameFailePopUp;
        failPopUp.SetUp();

        // failPopUp.SetUp(currentStage.level);
    }

    [ContextMenu("TestGameOver")]
    public void TestGameOver()
    {
        GameEnd(false).Forget();
    }

    [ContextMenu("TestGameClear")]
    public void TestGameClear()
    {
        GameEnd(true).Forget();
    }

    async UniTask GameClear()
    {
        PopupBase popup = PopupManager.Instance.ShowPopup(PopupType.GameSuccess);
        GameSuccessPopUp successPopup = popup as GameSuccessPopUp;
        
        successPopup.SetUp();

        await UniTask.Delay(500);

    }

    public void Pause()
    {
        isGameActive = false;
        OnGamePause?.Invoke(true);
    }
    
    public void Resume()
    {
        isGameActive = true;
        OnGamePause?.Invoke(false);
    }
    
}
