using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 게임 내의 기본 스탯을 저장하는 스크립트입니다.
/// 영구적인 변경이나, 임시 변경 등으로 인해 값이 변하지 않습니다.
/// </summary>
[CreateAssetMenu(fileName = "Game Data", menuName = "GameData/GameDatabase")]
public class GameDataDataBase : ScriptableObject
{
    [Header("기본 스탯 레벨 테이블")]
    [SerializeField] private BaseStatLevelTable levelTable;

    [Header("기본 스탯 정의 목록")]
    [SerializeField] private List<StatData> baseStat = new List<StatData>();

    private Dictionary<StatType, Stat> stats = new Dictionary<StatType, Stat>();

    public void Initialize()
    {
        stats = new Dictionary<StatType, Stat>(baseStat.Count);
        Debug.Log("게임 데이터 베이스 초기화");
        foreach (var stat in baseStat)
        {
            if (stat != null && !stats.ContainsKey(stat.type))
            {
                stats.Add(stat.type, new Stat(stat.type, stat.IconName, stat.statName)); 
            }
        }
    }

    /// <summary>
    /// [핵심 기능] 스탯 ID와 레벨을 기반으로 실제 스탯 값을 가져옵니다.
    /// (예: GetStatValue("MAX_HEALTH", 5) -> 140 반환)
    /// </summary>
    /// <param name="type">"MAX_HEALTH"와 같은 Stat Type</param>
    /// <param name="level">플레이어의 현재 스탯 레벨 (1부터 시작)</param>
    /// <returns>레벨에 해당하는 스탯 값</returns>
    public float GetStatValue(StatType type, int level)
    {
        if (levelTable == null)
        {
            Debug.LogError("[GameDataDatabase] Level Table이 연결되지 않았습니다!");
            return 0f;
        }

        if (!stats.ContainsKey(type))
        {
            Debug.LogWarning($"[GameDataDatabase] '{type}'는 유효하지 않은 스탯 ID입니다.");
            return 0f;
        }
        return levelTable.GetStatValueAtLevel(type.ToString(), level);
    }

    /// <summary>
    /// type에 대한 스탯을 반환합니다.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public Stat GetStat(StatType type)
    {
        if (stats.ContainsKey(type))
        {
            return stats[type];
        }
        Debug.LogWarning($"[GameDataDatabase] '{type}'는 유효하지 않은 스탯 ID입니다.");
        return null;
    }

    /// <summary>
    /// [핵심 기능] 스탯 목록을 반환합니다.
    /// </summary>
    /// <returns>스탯 목록</returns>
    public Dictionary<StatType, Stat> GetStats()
    {
        return stats;
    }
}
