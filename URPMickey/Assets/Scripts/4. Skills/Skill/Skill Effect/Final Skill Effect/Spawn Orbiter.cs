using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect_SpawnOrbiter", menuName = "Skill/Effect/Spawn Orbiter")]
public class SpawnOrbiter : SkillEffect
{
    [Header("프리팹")]
    [SerializeField] private GameObject orbiterPrefab; // 'Orbiter.cs' 스크립트가 붙어있어야 함
   
    [Header("기본 스탯")]
    [SerializeField] private int count = 3;
    [SerializeField] private float radius = 0.5f;
    [SerializeField] private float rotationSpeed = 60f;

    [Header("공전체 개별 스탯")]
    [SerializeField] private float baseDamage = 5f;
    [SerializeField] private float baseHealth = 20f;

    public override void Execute(SkillContext context)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject orb = Instantiate(orbiterPrefab, context.user.transform.position, Quaternion.identity);
            Orbiter orbScript = orb.GetComponent<Orbiter>();

            if (orbScript != null)
            {
                float startAngle = 360f / count * i;
                orbScript.Initialize(context.user.transform, radius, startAngle, rotationSpeed, baseDamage, baseHealth);
            }
        }
    }
}
