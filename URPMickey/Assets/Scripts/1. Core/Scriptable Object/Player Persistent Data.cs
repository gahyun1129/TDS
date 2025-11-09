using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// 플레이어의 영구 저장 데이터를 정의하는 순수 C# 클래스입니다.
/// [System.Serializable] 어트리뷰트가 있어야 JsonUtility가 직렬화할 수 있습니다.
/// </summary>
[System.Serializable]
public class PlayerPersistentData
{
    public int currency;
    public int highestWave;

    public List<StatLevelEntry> statLevels;

    public PlayerPersistentData()
    {
        currency = 0;
        highestWave = 1;
        statLevels = new List<StatLevelEntry>();

        foreach (StatType stat in (StatType[])Enum.GetValues(typeof(StatType)))
        {
            StatLevelEntry entry = new StatLevelEntry(stat.ToString(), 1);
            statLevels.Add(entry);
        }
    }

    public int GetStatLevel(StatType type)
    {
        if (statLevels == null)
        {
            statLevels = new List<StatLevelEntry>();
            return 1;
        }

        foreach (var entry in statLevels)
        {
            if (entry.statId == type.ToString())
            {
                return entry.level;
            }
        }
        return 1;
    }

    public void SetStatLevel(StatType type, int level)
    {

        string sType = type.ToString();
        if (statLevels == null)
        {
            statLevels = new List<StatLevelEntry>();
        }

        for (int i = 0; i < statLevels.Count; i++)
        {
            if (statLevels[i].statId == sType)
            {
                statLevels[i] = new StatLevelEntry(sType, level);
                return;
            }
        }

        statLevels.Add(new StatLevelEntry(sType, level));
    }
    
}

/// <summary>
/// 딕셔너리 대신 JSON으로 직렬화하기 위한 헬퍼 구조체(Struct)입니다.
/// </summary>
[System.Serializable]
public struct StatLevelEntry
{
    public string statId;
    public int level;

    public StatLevelEntry(string id, int lvl)
    {
        statId = id;
        level = lvl;
    }
}