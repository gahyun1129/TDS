using UnityEngine;
using System.Collections.Generic;

// 플레이어에게 필요한 컴포넌트들
[RequireComponent(typeof(Animator))]
public class SkillManager : MonoBehaviour
{
    public List<SkillData> equippedSkills;

    private Dictionary<SkillData, float> skillCooldowns = new Dictionary<SkillData, float>();

    public Animator anim { get; private set; }

    public GameObject currentTarget;

    public Vector3 GetTargetPoint()
    {
        if (currentTarget != null)
        {
            return currentTarget.transform.position;
        }
        return transform.position + transform.forward * 10f;
    }


    private void Awake()
    {
        anim = GetComponent<Animator>();
    
        foreach (var skill in equippedSkills)
        {
            skillCooldowns[skill] = 0f;
            skill.UpdateSkillInfo(null);
        }
    }

    /// <summary>
    /// 지정된 슬롯의 스킬을 사용합니다.
    /// </summary>
    public bool UseSkill(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= equippedSkills.Count)
        {
            Debug.LogWarning($"잘못된 스킬 슬롯: {slotIndex}");
            return false;
        }

        SkillData skill = equippedSkills[slotIndex];

        if (Time.time < skillCooldowns.GetValueOrDefault(skill, 0f))
        {
            // Debug.Log($"{skill.skillName} 쿨타임 중입니다.");
            return false;
        }

        string failMessage;
        if (!skill.CanUse(this, out failMessage))
        {
            Debug.Log($"스킬 사용 실패: {skill.finalSkillName} - {failMessage}");
            return false;
        }

        skill.ExecuteCosts(this);

        Debug.Log($"스킬 사용: {skill.finalSkillName}");
        skill.ExecuteEffects(this);

        skillCooldowns[skill] = Time.time + skill.cooldown;
        return true;
    }

    public void EquipSkill(SkillData skill)
    {
        if (!equippedSkills.Contains(skill))
        {
            equippedSkills.Add(skill);
            skillCooldowns[skill] = 0f;
        }
    }

    public void UnequipSkill(SkillData skill)
    {
        if (equippedSkills.Contains(skill))
        {
            equippedSkills.Remove(skill);
            skillCooldowns.Remove(skill);
        }
    }
    
    public void OnGameEnd()
    {
        foreach ( var skill in equippedSkills)
        {
            skill.ResetRune();
        }
    }
}