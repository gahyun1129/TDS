using UnityEngine;

/// <summary>
/// 던지는 스킬 사용 시, 던져야 할 물체
/// launch: 던지기 시작
/// explode: 폭발(destroy)
/// (디버깅용) OnDrawGizmosSelected
/// </summary>
public abstract class Projectile : MonoBehaviour
{

    [Header("딜")]
    public float areaOfEffectRadius;
    public float effectDuration;
    public GameObject explosionVFX;

    protected SkillContext context;

    public virtual void Launch(SkillContext _context)
    {
        context = _context;
    }

    public abstract void Explode();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; // 탐지 범위
        Gizmos.DrawWireSphere(transform.position, areaOfEffectRadius);
    }

}