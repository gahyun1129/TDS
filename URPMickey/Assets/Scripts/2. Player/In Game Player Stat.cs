using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// '인게임' 씬의 캐릭터에 부착되어 '실시간' 능력치를 담당하는 스크립트입니다.
/// '영구 저장 스탯(Player Stats Manager)' 스크립트의 값을 복사하여
/// 현재(current) 스탯을 초기화합니다.
/// 버프, 디버프, 장비 아이템 효과 등 '임시적인' 모든 스탯 변경은
/// 이 스크립트 내에서만 처리되며, 씬이 종료되면 사라집니다.
/// </summary>
public class InGamePlayerStat : MonoBehaviour
{
    public static InGamePlayerStat Instance { get; private set; }

    private Dictionary<StatType, float> playerStat = new Dictionary<StatType, float>();

    [Header("현재 상태")]
    [SerializeField] private float currentHP;
    [SerializeField] private float currentMana;
    [SerializeField] private bool isDead;

    public event Action<float, float> OnUpdateHP;
    public event Action<float, float> OnUpdateMana;
        

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    public bool IsDead => isDead;
    public float HP => currentHP;
    public float Mana => currentMana;

    #region
    // 방어
    public float MaxHealth => playerStat[StatType.MAX_HEALTH];
    public float Defense => playerStat[StatType.DEFENSE];
    public float DefensePenetration => playerStat[StatType.DEFENSE_PENETRATION];
    public float Evasion => playerStat[StatType.EVASION];
    public float Regeneration => playerStat[StatType.REGENERATION];
    public float Resistance => playerStat[StatType.RESISTANCE];
        
    // 공격
    public float AttackPower => playerStat[StatType.ATTACK_POWER];
    public float AttackSpeed => playerStat[StatType.ATTACK_SPEED];
    public float CriticalChance => playerStat[StatType.CRITICAL_CHANCE];
    public float CriticalDamage => playerStat[StatType.CRITICAL_DAMAGE];
    public float LifeSteal => playerStat[StatType.LIFESTEAL];

    // 유틸리티
    public float Luck => playerStat[StatType.LUCK];
    public float ManaReduction => playerStat[StatType.MANA_REDUCTION];
    public float ManaRegeneration => playerStat[StatType.MANA_REGENERATION];
    public float MaxMana => playerStat[StatType.MAX_MANA];
    public float Movement_Speed => playerStat[StatType.MOVEMENT_SPEED];
    #endregion

    void Start()
    {
        foreach (StatType stat in (StatType[])Enum.GetValues(typeof(StatType)))
        {
            playerStat[stat] = GameManager.Instance.GetValueOfStat(stat);
        }

        currentHP = playerStat[StatType.MAX_HEALTH];
        currentMana = playerStat[StatType.MAX_MANA];
        isDead = false;
    }

    void Update()
    {
        RegenerateHealth();
        RegenerateMana();
    }

    // -------------------------------------------------------------------
    // 기능별 함수
    // -------------------------------------------------------------------

    public void DealDamageTo(IDamageable target, float skillBaseDamage)
    {
        // 1. 기본 데미지 계산 (공격력 + 스킬 데미지)
        float baseDamage = AttackPower + skillBaseDamage;

        // 3. 치명타 적용
        if (CheckCriticalHit())
        {
            baseDamage = ApplyCriticalDamage(baseDamage);
            Debug.Log("치명타 적용!");
        }

        // 4. 대상의 유효 방어력 계산 (방어력 - 방어 관통)
        float effectiveDefense = CalculateEffectiveDefense(target.Defense);

        // 5. 최종 데미지 계산 (방어력 공식 적용)
        // 예시 공식: Damage = BaseDamage * (100 / (100 + EffectiveDefense))
        // 간단한 공식: Damage = BaseDamage - EffectiveDefense
        float finalDamage = Mathf.Max(1, baseDamage - effectiveDefense); // 최소 1 데미지

        target.TakeDamage(finalDamage);

        ApplyLifeSteal(finalDamage);
    }

    public void TakeDamage(float incomingDamage)
    {
        if (CheckEvasion())
        {
            Debug.Log("회피!");
            return;
        }

        float damageAfterDefense = ApplyDefense(incomingDamage);

        float finalDamage = ApplyResistance(damageAfterDefense);

        currentHP -= finalDamage;

        if (currentHP <= 0) { isDead = true; }

        Debug.Log($"기본 데미지: {incomingDamage} | 방어력 적용 후: {damageAfterDefense} | 저항 적용 후: {finalDamage}");
        OnUpdateHP?.Invoke(HP, MaxHealth);
    }
    
    /// <summary>
    /// 스킬 사용 시 마나를 소모합니다. (마나 감소 스탯 적용)
    /// </summary>
    /// <param name="manaCost">스킬의 기본 마나 소모량</param>
    /// <returns>마나 사용 성공 여부</returns>
    public bool TryUseMana(float manaCost)
    {
        // 1. 최종 마나 소모량 계산
        float finalCost = manaCost; //= CalculateFinalManaCost(manaCost);

        // 2. 마나 확인
        if (currentMana >= finalCost)
        {
            currentMana -= finalCost;
            OnUpdateMana?.Invoke(currentMana, MaxMana);
            return true;
        }
        else
        {
            Debug.Log("마나가 부족합니다.");
            return false;
        }
    }

    public void RegenerateHealth()
    {
        Heal(Regeneration * TimeManager.Instance.DeltaTime);
    }

    public void RegenerateMana()
    {
        RestoreMana(ManaRegeneration * TimeManager.Instance.DeltaTime);
    }

    // --- 개별 계산 함수 (Helper Functions) ---
    // [공격 관련 헬퍼]

    private bool CheckCriticalHit()
    {
        // 행운 스탯이 치명타 확률을 보정 (예: 행운 100당 1% 증가)
        float effectiveChance = CriticalChance + (Luck * 0.0001f);
        return UnityEngine.Random.Range(0f, 1f) <= effectiveChance;
    }

    private float ApplyCriticalDamage(float baseDamage)
    {
        return baseDamage * CriticalDamage;
    }

    /// <summary>
    /// 대상의 방어력에 플레이어의 방어 관통을 적용한 유효 방어력을 계산합니다.
    /// </summary>
    private float CalculateEffectiveDefense(float targetDefense)
    {
        return Mathf.Max(0, targetDefense - DefensePenetration); // 0 미만으로 내려가지 않음
    }

    private void ApplyLifeSteal(float damageDealt)
    {
        float stealAmount = damageDealt * LifeSteal;
        Debug.Log($"데미지 크기: {damageDealt} | 흡혈한 양: {LifeSteal}");
        Heal(stealAmount);
    }

    // [방어 관련 헬퍼]

    private bool CheckEvasion()
    {
        // 행운 스탯이 회피율을 보정 (예: 행운 100당 1% 증가)
        float effectiveEvasion = Evasion + (Luck * 0.0001f);
        return UnityEngine.Random.Range(0f, 1f) <= effectiveEvasion;
    }

    private float ApplyDefense(float incomingDamage)
    {
        // 간단한 뺄셈 공식: 
        return Mathf.Max(1, incomingDamage - Defense); // 최소 1 데미지
    }

    /// <summary>
    /// (방어력 적용 후) 데미지에 저항(%)을 적용합니다.
    /// </summary>
    private float ApplyResistance(float damageAfterDefense)
    {
        // Resistance (0.0 ~ 1.0) 만큼 데미지 감소
        return damageAfterDefense * (1.0f - Resistance);
    }

    // [유틸리티 헬퍼]

    /// <summary>
    /// 마나 감소 스탯이 적용된 최종 마나 소모량을 계산합니다.
    /// </summary>
    private float CalculateFinalManaCost(float baseCost)
    {
        // ManaReduction (0.0 ~ 1.0) 만큼 소모량 감소
        return baseCost * (1.0f - ManaReduction);
    }

    public void Heal(float amount)
    {
        currentHP = Mathf.Min(MaxHealth, currentHP + amount);
        OnUpdateHP?.Invoke(currentHP, MaxHealth);
    }

    public void RestoreMana(float amount)
    {
        currentMana = Mathf.Min(MaxMana, currentMana + amount);
        OnUpdateMana?.Invoke(currentMana, MaxMana);
    }

}
