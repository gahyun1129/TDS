using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct StatModifier
{
    public float value;
    public StatModType type;
    public StatType stat;

    public object source;

    public StatModifier(float _value, StatModType _type, StatType _stat, object _source)
    {
        value = _value;
        type = _type;
        stat = _stat;
        source = _source;
    }
}

public enum StatModType
{
    Flat = 100,             // 기본값에 더하거나 빼는 방식
    PercentAdd = 200,       // 기본값에 %를 더하거나 빼는 방식
    PercentMult = 300,      // 최종값에 %를 곱하는 방식
}

public enum StatType
{
    MAX_HEALTH,
    MAX_MANA,
    ATTACK_POWER,
    MANA_REDUCTION,
    DEFENSE,
    CRITICAL_CHANCE,
    RESISTANCE,
    CRITICAL_DAMAGE,
    REGENERATION,
    ATTACK_SPEED,
    LIFESTEAL,
    DEFENSE_PENETRATION,
    EVASION,
    MOVEMENT_SPEED,
    MANA_REGENERATION,
    LUCK
}