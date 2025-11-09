using UnityEngine;

[CreateAssetMenu(fileName = "Effect_SpawnVFX", menuName = "Skill/Effect/Spawn VFX")]
public class SpawnVFXEffect : SkillEffect
{
    public GameObject vfxPrefab;
    public float destroyDelay = 2f;
    
    public enum SpawnLocation
    {
        OnUser,
        OnTarget,
        AtTargetPoint
    }
    public SpawnLocation location = SpawnLocation.OnUser;

    public override void Execute(SkillContext context)
    {
        if (vfxPrefab == null) return;

        Vector3 spawnPos = context.user.transform.position;
        if (location == SpawnLocation.OnTarget && context.user.currentTarget != null)
        {
            spawnPos = context.user.currentTarget.transform.position;
        }
        else if (location == SpawnLocation.AtTargetPoint)
        {
            spawnPos = context.user.GetTargetPoint();
        }

        GameObject vfx = Instantiate(vfxPrefab, spawnPos, Quaternion.identity);
        Destroy(vfx, destroyDelay);
    }
}