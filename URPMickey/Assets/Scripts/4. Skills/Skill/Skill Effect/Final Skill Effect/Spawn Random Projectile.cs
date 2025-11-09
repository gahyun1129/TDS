using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect_SpawnRandomProjectile", menuName = "Skill/Effect/Spawn Random Projectile")]
public class SpawnRandomProjectile : SkillEffect
{
    [Header("번개 생성 위치 지정")]
    [SerializeField] private float spawnHeight = 2f;
    [SerializeField] private float spawnRadius = 1f;

    private Vector3 GetRandomStrikePosition(SkillManager user)
    {
        Vector3 center = user.transform.position;
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        return center + new Vector3(offset.x, offset.y, 0);
    }

    public override void Execute(SkillContext context)
    {
        Vector3 targetPoint = GetRandomStrikePosition(context.user);
        Vector3 spawnPos = targetPoint + Vector3.up * spawnHeight;
        GameObject proj = Instantiate(context.projectile, spawnPos, Quaternion.identity);
        
        var pScript = proj.GetComponent<Projectile>();
        if (pScript != null)
        {
            pScript.Launch(context);
        }
    }
}
