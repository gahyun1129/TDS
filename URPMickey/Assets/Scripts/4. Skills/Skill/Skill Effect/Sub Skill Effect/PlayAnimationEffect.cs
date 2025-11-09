using UnityEngine;

[CreateAssetMenu(fileName = "Effect_PlayAnim", menuName = "Skill/Effect/Play Animation")]
public class PlayAnimationEffect : SkillEffect
{
    public string animationTriggerName;

    public override void Execute(SkillContext context)
    {
        if (context.user.anim != null && !string.IsNullOrEmpty(animationTriggerName))
        {
            context.user.anim.SetTrigger(animationTriggerName);
        }
    }
}
