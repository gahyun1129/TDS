using UnityEngine;

/// <summary>
/// 스킬 효과의 기본 청사진입니다.
/// ScriptableObject이므로 상속받은 클래스도 [CreateAssetMenu]가 가능합니다.
/// </summary>
public abstract class SkillEffect : ScriptableObject
{
    /// <summary>
    /// 이 효과를 실행합니다.
    /// </summary>
    /// <param name="user">스킬을 시전한 SkillManager</param>
    public abstract void Execute(SkillContext context);
}