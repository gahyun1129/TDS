using UnityEngine;
using DG.Tweening;

public class AOEProjectile : Projectile
{
    [Header("공격체 포물선")]
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private int jumpNum = 1;
    [SerializeField] private float jumpDuration = 1f;
    
    public override void Launch(SkillContext context)
    {
        base.Launch(context);

        Vector2 targetPos = context.user.GetTargetPoint();

        transform.DOJump(
            targetPos,
            jumpHeight,
            jumpNum,
            jumpDuration
        )
        .SetEase(Ease.Linear)
        .OnComplete(Explode);
    }

    public override void Explode()
    {
        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, effectDuration);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, areaOfEffectRadius);
        foreach (var hit in hits)
        {
            IDamageable damageableObject = hit.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                InGamePlayerStat.Instance.DealDamageTo(damageableObject, context.cumulativeDamage);
            }
        }

        Destroy(gameObject);
    }
}