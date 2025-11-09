using UnityEngine;

/// <summary>
/// 스킬 사용 조건의 기본 청사진입니다.
/// </summary>
public abstract class SkillRequirement : ScriptableObject
{
    /// <summary>
    /// 이 조건을 만족하는지 확인합니다.
    /// </summary>
    public abstract bool Check(SkillManager user);

    /// <summary>
    /// 조건을 만족하기 위한 비용을 지불합니다. (예: 마나 소모)
    /// </summary>
    public abstract void ExecuteCost(SkillManager user);
    
    /// <summary>
    /// 조건 미충족 시 표시할 메시지입니다.
    /// </summary>
    public abstract string GetErrorMessage();
}