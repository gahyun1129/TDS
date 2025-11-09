using UnityEngine;
using DG.Tweening;

public class SingleProjectile : Projectile
{
    [Header("공격체 이동")]
    [SerializeField] private float moveDuration = 1f;

    GameObject target;

    public override void Launch(SkillContext context)
    {
        base.Launch(context);

        Vector2 targetPos = context.user.GetTargetPoint();

        transform.DOMove(targetPos, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(Explode);

    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == context.user) return;

        if (collision.CompareTag("Enemy"))
        {
            if ( collision != null)
            {
                target = collision.gameObject;
                Explode();
            }
            // GetComponent<Collider2D>().enabled = false;
            // Destroy(gameObject, 1f);
        }
    }
    
    public override void Explode()
    {
        if (explosionVFX != null)
        {
            GameObject vfx = Instantiate(explosionVFX, transform.position, Quaternion.identity);
            Destroy(vfx, effectDuration);
        }

        if ( target != null)
        {
            IDamageable damageableObject = target.GetComponent<IDamageable>();
            if (damageableObject != null)
            {
                InGamePlayerStat.Instance.DealDamageTo(damageableObject, context.cumulativeDamage);
            }
        }

        Destroy(gameObject);
    }
}