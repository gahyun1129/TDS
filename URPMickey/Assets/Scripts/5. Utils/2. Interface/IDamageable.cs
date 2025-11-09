public interface IDamageable
{
    float CurrentHealth { get; }
    float Defense { get; } // 대상의 방어력 스탯
    void TakeDamage(float damage);
}