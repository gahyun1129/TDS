using DG.Tweening;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect_SpawnProjectile", menuName = "Skill/Effect/Spawn Projectile")]
public class SpawnProjectileEffect : SkillEffect
{
    public override void Execute(SkillContext context)
    {
        Vector3 spawnPos = context.user.transform.position + Vector3.up * 0.2f;
        GameObject proj= Instantiate(context.projectile, spawnPos, context.user.transform.rotation);

        var pScript = proj.GetComponent<Projectile>();
        if (pScript != null)
        {
            pScript.Launch(context);
        }      
    }
}