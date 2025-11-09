using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Effect_CinematicStrike", menuName = "Skill/Effect/Cinematic Chain Strike")]
public class CinematicChainStrikeEffect : SkillEffect
{
    [Header("스킬 기본 설정")]
    [SerializeField] private float range = 5f;
    [SerializeField] private int maxTargets = 8;
    [SerializeField] private LayerMask enemyLayer;

    [Header("공격 및 연출")]
    [SerializeField] private float damagePerHit = 50f;
    [SerializeField] private string attackTriggerName = "Attack"; 
    [SerializeField] private float delayBetweenHits = 0.15f; 
    [SerializeField] private GameObject vfxOnHit;
    [SerializeField] private float attackOffset = 0.5f;

    [Header("적 멈춤 설정")]
    public float stunDuration = 2.0f;

    public override void Execute(SkillContext context)
    {
        // SkillManager의 MonoBehaviour 기능을 빌려 코루틴을 실행합니다.
        context.user.StartCoroutine(StrikeCoroutine(context.user));
    }
    private IEnumerator StrikeCoroutine(SkillManager user)
    {
        // --- 0. 시전 준비 ---
        Vector3 originalPosition = user.transform.position; // 스킬 종료 후 돌아올 위치
        Vector3 originalScale = user.transform.localScale; // (추가) 원래 스케일 저장

        // --- 1. 주변의 모든 적 탐색 (2D 변경점) ---
        // Physics.OverlapSphere 대신 Physics2D.OverlapCircleAll 사용
        Collider2D[] hits = Physics2D.OverlapCircleAll(user.transform.position, range, enemyLayer);

        List<Health> targets = hits
            .Select(hit => hit.GetComponent<Health>()) // Health 스크립트는 공용
            .Where(h => h != null && h.hp > 0)
            .OrderBy(h => Vector2.Distance(user.transform.position, h.transform.position)) // 2D 거리 계산
            .Take(maxTargets)
            .ToList();

        if (targets.Count == 0) yield break;

        List<Animator> stunnedAnimators = new List<Animator>();
        List<Rigidbody2D> stunnedRigidbodies = new List<Rigidbody2D>();
        List<float> originalGravityScales = new List<float>(); // 중력값도 원래대로 돌려놔야 함

        foreach (var target in targets)
        {
            var anim = target.GetComponent<Animator>();
            if (anim != null)
            {
                anim.speed = 0;
                stunnedAnimators.Add(anim);
            }

            target.GetComponent<EnemyAutoAI>().SetIsDamaged(true);
        }

        int targetIndex = 0; 
        
        foreach (var target in targets)
        {
            if (target == null) continue; 
            
            Vector2 targetPos2D = target.transform.position;
            
            Vector2 offsetDirection = (targetIndex % 2 == 0) ? Vector2.left : Vector2.right;
            
            Vector2 attackPos2D = targetPos2D + (offsetDirection * attackOffset);

            user.transform.position = new Vector3(attackPos2D.x, attackPos2D.y, originalPosition.z);

            Vector2 directionToTarget = (targetPos2D - attackPos2D).normalized; // (항상 Vector2.right 아니면 .left가 됨)
            float lookDirectionSign = Mathf.Sign(directionToTarget.x);
            
            user.transform.localScale = new Vector3(
                Mathf.Abs(originalScale.x) * lookDirectionSign, // 원래 스케일의 x 절대값 사용
                originalScale.y, 
                originalScale.z);

            // "쇽" (공격)
            if (user.anim != null && !string.IsNullOrEmpty(attackTriggerName))
            {
                user.anim.SetTrigger(attackTriggerName);
            }
            if (vfxOnHit != null)
            {
                Instantiate(vfxOnHit, target.transform.position, Quaternion.identity);
            }
            InGamePlayerStat.Instance.DealDamageTo(target.gameObject.GetComponent<IDamageable>(), damagePerHit);

            // "쇽" (대기)
            yield return new WaitForSeconds(delayBetweenHits);
            
            targetIndex++; // 다음 타겟 인덱스
        }

        // --- 4. 스킬 종료 및 뒷정리 ---
        user.transform.position = originalPosition; // 원위치 복귀
        user.transform.localScale = originalScale;  // (추가) 원래 방향(스케일) 복귀

        foreach (var target in targets)
        {
            target.GetComponent<EnemyAutoAI>().SetIsDamaged(false);
        }
        
        // 멈췄던 적들 애니메이터 복구
        foreach (var anim in stunnedAnimators)
        {
            if (anim != null) anim.speed = 1;
        }
    }
}
