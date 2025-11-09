using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Requirement_UseMana", menuName = "Skill/Requirement/Use Mana")]
public class UseMana : SkillRequirement
{
    [Header("기본 정보")]
    [SerializeField] private float requiredMana;

    public override bool Check(SkillManager user)
    {
        if (requiredMana <= InGamePlayerStat.Instance.Mana)
        {
            return true;
        }
        return false;
    }

    public override void ExecuteCost(SkillManager user)
    {
        InGamePlayerStat.Instance.TryUseMana(requiredMana);
        Debug.Log($"마나 사용: {requiredMana}");
    }

    public override string GetErrorMessage()
    {
        return "마나가 부족합니다.";
    }
}
