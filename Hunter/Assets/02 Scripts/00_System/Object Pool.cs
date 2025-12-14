using UnityEngine;
using UnityEngine.Pool; // ★ 필수 네임스페이스

public class SpearPoolManager : MonoBehaviour
{
    public static SpearPoolManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private SpearProjectile spearPrefab;
    [SerializeField] private int defaultCapacity = 10;
    [SerializeField] private int maxPoolSize = 50;

    // 유니티 내장 오브젝트 풀
    private IObjectPool<SpearProjectile> _pool;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 풀 초기화 (생성, 가져올때, 돌려줄때, 삭제할때 로직 등록)
        _pool = new ObjectPool<SpearProjectile>(
            createFunc: CreateSpear,
            actionOnGet: OnGetSpear,
            actionOnRelease: OnReleaseSpear,
            actionOnDestroy: OnDestroySpear,
            collectionCheck: true, // 이미 반환된 걸 또 반환하려 하면 에러 띄움 (디버깅용)
            defaultCapacity: defaultCapacity,
            maxSize: maxPoolSize
        );
    }

    private void Start()
    {
        // 풀 프리워밍: 미리 defaultCapacity만큼 생성해두기
        PrewarmPool();
    }

    private void PrewarmPool()
    {
        // 임시로 오브젝트들을 가져왔다가 바로 반납
        SpearProjectile[] tempSpears = new SpearProjectile[defaultCapacity];
        
        for (int i = 0; i < defaultCapacity; i++)
        {
            tempSpears[i] = _pool.Get();
        }
        
        for (int i = 0; i < defaultCapacity; i++)
        {
            _pool.Release(tempSpears[i]);
        }
        
        Debug.Log($"[SpearPoolManager] 풀 프리워밍 완료: {defaultCapacity}개 생성");
    }

    // 1. 실제 생성 (풀이 비었을 때)
    private SpearProjectile CreateSpear()
    {
        SpearProjectile spear = Instantiate(spearPrefab, transform);
        spear.SetPool(_pool); // 투사체에게 "너는 이 풀 소속이야"라고 알려줌
        return spear;
    }

    // 2. 풀에서 꺼내갈 때 (SetActive true)
    private void OnGetSpear(SpearProjectile spear)
    {
        spear.gameObject.SetActive(true);
        // 여기서 초기화 로직은 SpearProjectile 내부에서 처리하는 게 깔끔함
    }

    // 3. 풀로 반납할 때 (SetActive false)
    private void OnReleaseSpear(SpearProjectile spear)
    {
        spear.gameObject.SetActive(false);
    }

    // 4. 풀이 꽉 찼는데 반납 들어오면 삭제
    private void OnDestroySpear(SpearProjectile spear)
    {
        Destroy(spear.gameObject);
    }

    // --- 외부 사용 함수 ---
    public SpearProjectile GetSpear()
    {
        return _pool.Get();
    }
}
