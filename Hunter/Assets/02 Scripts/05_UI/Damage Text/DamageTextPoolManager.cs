using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;

/// <summary>
/// 데미지 텍스트 오브젝트 풀 매니저
/// 3가지 타입의 데미지 텍스트를 관리 (Hat, Normal, Critical)
/// </summary>
public class DamageTextPoolManager : MonoBehaviour
{
    public static DamageTextPoolManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private DamagePrefab hatDamagePrefab;
    [SerializeField] private DamagePrefab normalDamagePrefab;
    [SerializeField] private DamagePrefab criticalDamagePrefab;
    [SerializeField] private DamagePrefab comboDamagePrefab;

    [Header("Pool Settings")]
    [SerializeField] private int initialPoolSize = 5; // 각 타입당 초기 생성 개수
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 50;

    [Header("Parent Transform")]
    [SerializeField] private Transform damageTextParent; // UI Canvas 하위에 배치

    // 각 타입별 오브젝트 풀
    private IObjectPool<DamageTextBase> _hatPool;
    private IObjectPool<DamageTextBase> _normalPool;
    private IObjectPool<DamageTextBase> _criticalPool;
    private IObjectPool<DamageTextBase> _comboPool;

    // 미리 생성된 오브젝트 보관용
    private List<DamageTextBase> _preCreatedHatTexts = new List<DamageTextBase>();
    private List<DamageTextBase> _preCreatedNormalTexts = new List<DamageTextBase>();
    private List<DamageTextBase> _preCreatedCriticalTexts = new List<DamageTextBase>();
    private List<DamageTextBase> _preCreatedComboTexts = new List<DamageTextBase>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 부모 Transform이 없으면 자동 생성
        if (damageTextParent == null)
        {
            GameObject parent = new GameObject("Damage Texts");
            parent.transform.SetParent(transform);
            damageTextParent = parent.transform;
        }

        InitializePools();
        PreCreateObjects();
    }

    /// <summary>
    /// 풀 초기화
    /// </summary>
    private void InitializePools()
    {
        // Hat Damage Pool
        _hatPool = new ObjectPool<DamageTextBase>(
            createFunc: () => CreateDamageText(hatDamagePrefab),
            actionOnGet: OnGetDamageText,
            actionOnRelease: OnReleaseDamageText,
            actionOnDestroy: OnDestroyDamageText,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );

        // Normal Damage Pool
        _normalPool = new ObjectPool<DamageTextBase>(
            createFunc: () => CreateDamageText(normalDamagePrefab),
            actionOnGet: OnGetDamageText,
            actionOnRelease: OnReleaseDamageText,
            actionOnDestroy: OnDestroyDamageText,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );

        // Critical Damage Pool
        _criticalPool = new ObjectPool<DamageTextBase>(
            createFunc: () => CreateDamageText(criticalDamagePrefab),
            actionOnGet: OnGetDamageText,
            actionOnRelease: OnReleaseDamageText,
            actionOnDestroy: OnDestroyDamageText,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );

        // Combo Pool
        _comboPool = new ObjectPool<DamageTextBase>(
            createFunc: () => CreateDamageText(comboDamagePrefab),
            actionOnGet: OnGetDamageText,
            actionOnRelease: OnReleaseDamageText,
            actionOnDestroy: OnDestroyDamageText,
            collectionCheck: true,
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );
    }

    /// <summary>
    /// 각 타입당 5개씩 미리 생성
    /// </summary>
    private void PreCreateObjects()
    {
        // Hat Damage 미리 생성
        for (int i = 0; i < initialPoolSize; i++)
        {
            DamageTextBase hatText = _hatPool.Get();
            _preCreatedHatTexts.Add(hatText);
        }
        foreach (var text in _preCreatedHatTexts)
        {
            _hatPool.Release(text);
        }
        _preCreatedHatTexts.Clear();

        // Normal Damage 미리 생성
        for (int i = 0; i < initialPoolSize; i++)
        {
            DamageTextBase normalText = _normalPool.Get();
            _preCreatedNormalTexts.Add(normalText);
        }
        foreach (var text in _preCreatedNormalTexts)
        {
            _normalPool.Release(text);
        }
        _preCreatedNormalTexts.Clear();

        // Critical Damage 미리 생성
        for (int i = 0; i < initialPoolSize; i++)
        {
            DamageTextBase criticalText = _criticalPool.Get();
            _preCreatedCriticalTexts.Add(criticalText);
        }
        foreach (var text in _preCreatedCriticalTexts)
        {
            _criticalPool.Release(text);
        }
        _preCreatedCriticalTexts.Clear();

        // Combo 미리 생성
        for (int i = 0; i < initialPoolSize; i++)
        {
            DamageTextBase comboText = _comboPool.Get();
            _preCreatedComboTexts.Add(comboText);
        }
        foreach (var text in _preCreatedComboTexts)
        {
            _comboPool.Release(text);
        }
        _preCreatedComboTexts.Clear();
    }

    /// <summary>
    /// 데미지 텍스트 생성
    /// </summary>
    private DamageTextBase CreateDamageText(DamagePrefab damage)
    {
        DamageTextBase damageText = Instantiate(damage.damageBase, damageTextParent);
        
        // 어떤 풀에 속하는지 설정
        if (damage.type == DamageType.Hat)
            damageText.SetPool(_hatPool);
        else if (damage.type == DamageType.Normal)
            damageText.SetPool(_normalPool);
        else if (damage.type == DamageType.Critical)
            damageText.SetPool(_criticalPool);
        else if (damage.type == DamageType.Combo)
            damageText.SetPool(_comboPool);

        return damageText;
    }

    /// <summary>
    /// 풀에서 꺼낼 때
    /// </summary>
    private void OnGetDamageText(DamageTextBase damageText)
    {
        damageText.gameObject.SetActive(true);
    }

    /// <summary>
    /// 풀로 반환할 때
    /// </summary>
    private void OnReleaseDamageText(DamageTextBase damageText)
    {
        damageText.gameObject.SetActive(false);
    }

    /// <summary>
    /// 풀이 꽉 찼을 때 삭제
    /// </summary>
    private void OnDestroyDamageText(DamageTextBase damageText)
    {
        Destroy(damageText.gameObject);
    }

    // ========== 외부 사용 함수 ==========

    /// <summary>
    /// Hat 데미지 텍스트 표시
    /// </summary>
    public void ShowHatDamage(float damage, Vector3 worldPosition)
    {
        DamageTextBase damageText = _hatPool.Get();
        damageText.Initialize(damage, worldPosition, true);
    }

    /// <summary>
    /// Normal 데미지 텍스트 표시
    /// </summary>
    public void ShowNormalDamage(float damage, Vector3 worldPosition)
    {
        DamageTextBase damageText = _normalPool.Get();
        damageText.Initialize(damage, worldPosition, true);
    }

    /// <summary>
    /// Critical 데미지 텍스트 표시
    /// </summary>
    public void ShowCriticalDamage(float damage, Vector3 worldPosition)
    {
        DamageTextBase damageText = _criticalPool.Get();
        damageText.Initialize(damage, worldPosition, true);
    }

    /// <summary>
    /// Combo 텍스트 표시 (프리팹에 설정된 위치 사용, 회전만 랜덤)
    /// </summary>
    public void ShowCombo(float damage)
    {
        DamageTextBase damageText = _comboPool.Get();
        damageText.Initialize(damage);
    }

    /// <summary>
    /// 데미지 타입에 따라 자동으로 표시
    /// </summary>
    public void ShowDamage(float damage, Vector3 worldPosition, DamageType damageType)
    {
        switch (damageType)
        {
            case DamageType.Hat:
                ShowHatDamage(damage, worldPosition);
                break;
            case DamageType.Normal:
                ShowNormalDamage(damage, worldPosition);
                break;
            case DamageType.Critical:
                ShowCriticalDamage(damage, worldPosition);
                break;
            case DamageType.Combo:
                ShowCombo(damage);
                break;
        }
    }
}

/// <summary>
/// 데미지 타입 열거형
/// </summary>
public enum DamageType
{
    Hat,        // 모자 데미지
    Normal,     // 일반 데미지
    Critical,   // 크리티컬 데미지
    Combo       // 콤보
}

[Serializable]
public struct DamagePrefab
{
    public DamageType type;
    public DamageTextBase damageBase;
}
