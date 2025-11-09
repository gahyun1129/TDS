using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    fire_ball,

}

[CreateAssetMenu(fileName = "Game Data", menuName = "GameData/Skill Data Base")]
public class SkillDataDataBase : ScriptableObject
{
    [Header("기본 스킬 정의 목록")]
    [SerializeField] private List<SkillData> baseSkill = new List<SkillData>();

    [Header("시너지 룬 정의 목록")]
    [SerializeField] private List<RuneData> runes;

    public RuneData GetRandomCog()
    {
        return runes[Random.Range(0, runes.Count)];
    }
}
