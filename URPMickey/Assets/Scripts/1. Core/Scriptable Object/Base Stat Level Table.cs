using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BaseStatLevelTable", menuName = "GameData/Base Stat Level Table")]
public class BaseStatLevelTable : ScriptableObject
{
    // C# 필드 이름이 BaseStatLevels 시트의 열 이름(TypeID)과 정확히 일치해야 합니다!
    // (리플렉션으로 매칭)
    [System.Serializable]
    public class LevelData
    {
        public int Level;
        public float MAX_HEALTH;
        public float MAX_MANA;
        public float ATTACK_POWER;
        public float MANA_REDUCTION;
        public float DEFENSE;
        public float CRITICAL_CHANCE;
        public float RESISTANCE;
        public float CRITICAL_DAMAGE;
        public float REGENERATION;
        public float ATTACK_SPEED;
        public float LIFESTEAL;
        public float DEFENSE_PENETRATION;
        public float EVASION;
        public float MOVEMENT_SPEED;
        public float MANA_REGENERATION;
        public float LUCK;
    }

    public List<LevelData> allLevelData = new List<LevelData>();

    private Dictionary<int, LevelData> _levelDataDict;

    public void Initialize()
    {
        _levelDataDict = new Dictionary<int, LevelData>();
        foreach (var data in allLevelData)
        {
            if (data != null)
            {
                _levelDataDict[data.Level] = data;
            }
        }
    }

    public LevelData GetLevelData(int level)
    {
        _levelDataDict.TryGetValue(level, out LevelData data);
        return data;
    }

    /// <summary>
    /// 런타임에 특정 스탯에 해당하는 특정 레벨 값을 반환하는 함수
    /// </summary>
    /// <param name="statTypeID">Stat Type .ToString() 변경 필수</param>
    /// <param name="level">해당하는 레벨</param>
    /// <returns></returns>
    public float GetStatValueAtLevel(string statTypeID, int level)
    {
        if (_levelDataDict == null) Initialize();
        
        if (_levelDataDict.TryGetValue(level, out LevelData levelData))
        {
            try
            {
                return (float)typeof(LevelData).GetField(statTypeID).GetValue(levelData);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BaseStatLevelTable] GetStatValueAtLevel FAILED: {statTypeID}. {e.Message}");
                return 0f;
            }
        }
        return 0f;
    }
}