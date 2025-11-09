using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class Stat
{
    public StatType type;
    [SerializeField] private float baseValue;
    [SerializeField] private int level;

    public string iconName;
    public string statName;

    public float BaseValue
    {
        get { return baseValue; }
        set
        {
            baseValue = value;
            RecalculateValue();
        }
    }

    public float CurrentValue { get; private set; }

    private readonly List<StatModifier> modifiers = new List<StatModifier>();

    public Stat(StatType _type, string _iconName, string _name)
    {
        statName = _name;
        iconName = _iconName;
        type = _type;
        RecalculateValue();
    }

    public Stat(Stat source)
    {
        this.statName = source.statName;
        this.iconName = source.iconName;
        this.type = source.type;
        this.baseValue = source.baseValue;

        this.modifiers.AddRange(source.modifiers);

        RecalculateValue();
    }

    public void RecalculateValue()
    {
        float finalValue = BaseValue;
        float percentAddSum = 0;

        foreach (var mod in modifiers.Where(m => m.type == StatModType.Flat))
        {
            finalValue += mod.value;
        }

        foreach (var mod in modifiers.Where(m => m.type == StatModType.PercentAdd))
        {
            percentAddSum += mod.value;
        }

        finalValue *= (1 + percentAddSum); // 기본값에 PercentAdd 합계를 적용

        foreach (var mod in modifiers.Where(m => m.type == StatModType.PercentMult))
        {
            finalValue *= (1 + mod.value);
        }

        CurrentValue = finalValue;
        OnStatChanged?.Invoke(type, CurrentValue);
    }

    public void AddModifier(StatModifier mod)
    {
        modifiers.Add(mod);

        modifiers.Sort((a, b) => a.type.CompareTo(b.type));
        RecalculateValue();
    }

    public bool RemoveModifier(StatModifier mod)
    {
        bool removed = modifiers.RemoveAll(m => m.source == mod.source && m.type == mod.type && m.value == mod.value && m.stat == mod.stat) > 0;
        if (removed)
        {
            RecalculateValue();
        }
        return removed;
    }
    
    public event Action<StatType, float> OnStatChanged;
}
