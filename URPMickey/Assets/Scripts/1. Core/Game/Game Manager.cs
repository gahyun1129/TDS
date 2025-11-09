using System.Collections.Generic;
using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("게임 기본 정보")]
    [SerializeField] private int stage = 0;
    [SerializeField] private int currency = 0;

    private Dictionary<StatType, StatLevelValue> stats = new Dictionary<StatType, StatLevelValue>();
    
    private GameDataDataBase dataBase;
    private PlayerPersistentData playerData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        dataBase = GameDataManager.Instance.GetDataBase();
        playerData = GameDataManager.Instance.GetPlayerData();
        currency = playerData.currency;

        foreach(var stat in playerData.statLevels)
        {
            if (   Enum.TryParse(stat.statId, out StatType type))
            {
                stats[type] = new StatLevelValue(dataBase.GetStat(type), stat.level, dataBase.GetStatValue(type, stat.level));
            }

        }
    }

    public void UpgradeStat()
    {
        StatType type = StatType.MAX_HEALTH;
        if (stats.ContainsKey(type))
        {
            stats[type].level += 1;
            int currentLevel = stats[type].level;

            stats[type].value = dataBase.GetStatValue(type, currentLevel);
            GameDataManager.Instance.UpgradeStat(type, currentLevel);
        }
    }
    /// <summary>
    /// 인게임 종료 후 호출합니다.
    /// 저장된 최고 기록보다 현재 스테이지가 높다면 새로 저장합니다.
    /// </summary>
    public void UpdateHighestWave()
    {
        if (stage > playerData.highestWave)
        {
            GameDataManager.Instance.UpdateHighestWave(stage);
        }
        stage = 0;
    }

    public float GetValueOfStat(StatType type)
    {
        if (stats.ContainsKey(type))
        {
            return stats[type].value;
        }

        return -1f;
    }

    public void GoToNextStage()
    {
        stage += 1;
    }
    public int STAGE => stage;
}

[System.Serializable]
public class StatLevelValue
{
    public Stat stat;
    public int level;
    public float value;

    public StatLevelValue(Stat _stat, int _level, float _value)
    {
        stat = _stat;
        level = _level;
        value = _value;
    }
}
