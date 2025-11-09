using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New SkillData", menuName = "Inventory/Skill Data")]
public class SkillData : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private string skillName;

    [TextArea]
    [SerializeField] private string description;

    [SerializeField] private Sprite icon;
    [SerializeField] private GameObject bsaeProjectile;
    public float cooldown;
    public float damage;

    [Header("보여지는 정보")]
    public string finalSkillName;
    public string findalDescription;
    public Sprite finalIcon;

    [Header("룬")]
    public List<RuneData> runes;

    [Header("발동 조건")]
    public List<SkillRequirement> requirements;

    [Header("스킬 효과(컴포넌트)")]
    public List<SkillEffect> effects;

    public bool CanUse(SkillManager user, out string failMessage)
    {
        foreach (var req in requirements)
        {
            if (!req.Check(user))
            {
                failMessage = req.GetErrorMessage();
                return false;
            }
        }
        failMessage = "";
        return true;
    }

    public void ExecuteCosts(SkillManager user)
    {
        foreach (var req in requirements)
        {
            req.ExecuteCost(user);
        }
    }

    public void ExecuteEffects(SkillManager user)
    {
        SkillContext context = new SkillContext(user);
        context.data = this;
        context.cumulativeDamage = damage;
        context.projectile = bsaeProjectile;

        List<SkillEffect> effectsToRun = new List<SkillEffect>(effects);

        if (runes.Count > 0)
        {
            foreach (var rune in runes)
            {
                if (rune.overrideProjectilePrefab != null)
                {
                    context.projectile = rune.overrideProjectilePrefab;
                }
                if (rune.additionalEffect != null)
                {
                    effectsToRun.Insert(0, rune.additionalEffect);
                }
                finalSkillName = rune.skillName;
                findalDescription = rune.description;
                finalIcon = rune.icon;
            }
        }
        else
        {
            finalSkillName = skillName;
            findalDescription = description;
            finalIcon = icon;
        }

        foreach (var effect in effectsToRun)
        {
            effect.Execute(context);
        }
    }

    public void EuippedRune(RuneData rune)
    {
        Debug.Log("룬 장착");
        runes.Add(rune);
        finalSkillName = rune.skillName;
        findalDescription = rune.description;
        finalIcon = rune.icon;
    }

    public void ResetRune()
    {
        runes.Clear();
    }
    
}

/// <summary>
/// 다음 스킬 이펙트와 공유해야 하는 정보
/// SkillManager user: 스킬 시전자
/// Vector3 target: 스킬 대상자
/// projectile: (선택) 던지는 스킬이라면 던져야 할 물체(프리팹)
/// SkillData data: 설명 및 스킬 이름 변경을 위해
/// float cumulativeDamage: 누적되는 데미지의 양 (후에 데미지 입힐 시, 플레이어의 데미지와 계산됨)
/// </summary>
[Serializable]
public class SkillContext
{
    public SkillManager user;

    public GameObject projectile;
    public SkillData data;

    public float cumulativeDamage;

    public SkillContext(SkillManager user)
    {
        this.user = user;
    }   
}