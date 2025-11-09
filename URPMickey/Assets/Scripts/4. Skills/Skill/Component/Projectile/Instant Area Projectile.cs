using UnityEngine;

public class InstantAreaProjectile : Projectile
{

    [Header("즉발 공격")]
    [SerializeField] private float duration;

    public override void Launch(SkillContext context)
    {
        base.Launch(context);
        Explode();
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

        Destroy(gameObject, duration);
    }
}
