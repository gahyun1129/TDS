using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class GameDataManager : MonoBehaviour
{
    #region singleton
    public static GameDataManager Instance { get; private set; }
    void Awake()
    {
        Application.targetFrameRate = 60;
        
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        currentLevelData = levelDatas[currentLevelIndex];
    }

    #endregion

    [Header("난이도별 데이터 파일 연결")]
    [SerializeField] public List<LevelData> levelDatas = new List<LevelData>();

    private int currentLevelIndex = 0;
    public LevelData currentLevelData { get; private set; }

    public void GoToNextStage()
    {
        currentLevelIndex++;
        if (currentLevelIndex >= levelDatas.Count)
        {
            currentLevelIndex = 0;
        }

        currentLevelData = levelDatas[currentLevelIndex];
    }
}

public enum GameDifficulty { Easy, Normal, Hard }