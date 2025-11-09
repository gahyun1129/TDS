using System.Collections;
using Unity.Mathematics;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    public static WaveManager Instance { get; private set; }

    [Header("적 생성 위치 지정")]
    [SerializeField] private Vector2 offset = new Vector2(1.5f, 1.5f);
    [SerializeField] private Vector2 center = new Vector2(0, 1f);
    [SerializeField] private float padding = 1f;

    [Header("웨이브 관련")]
    [SerializeField] private float waveTime;
    [SerializeField] private float restTime;
    [SerializeField] private int maxEnemyPerPortal;

    [Header("포탈 관련")]
    [SerializeField] private GameObject portalEffect;

    [Header("오브젝트 풀링 매니저")]
    [SerializeField] private ObjectPoolManager pool;

    public float gameTime { get; private set; }

    private GameObject player;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        player = BattleManager.Instance.Player;
        BattleManager.Instance.OnGameReady += ReadyToGame;
        BattleManager.Instance.OnGameStart += GameStart;
    }

    void OnDisable()
    {
        BattleManager.Instance.OnGameReady -= ReadyToGame;
        BattleManager.Instance.OnGameStart -= GameStart;
    }

    public void ReadyToGame(int stage)
    {
        gameTime = 0f;
        center = player.transform.position;
        BattleManager.Instance.ResetWaveStats();
    }
    
    public void GameStart()
    {
        StartCoroutine(IEnemySpawner());
    }

    public float WaveTime => waveTime;

    public Vector3 GetPortalPos()
    {
        center = player.transform.position;

        float x = UnityEngine.Random.Range(-offset.x + center.x, offset.x + center.x);
        x = x <= center.x ? x -= padding : x += padding;
        float y = UnityEngine.Random.Range(-offset.y + center.y, offset.y + center.y);
        y = y <= center.y ? y -= padding : y += padding;

        return new Vector3(x, y, 0);
    }

    private IEnumerator IEnemySpawner()
    {
        float delay;
        float timer;

        float spawnDuration = waveTime - restTime;

        while (gameTime < spawnDuration)
        {
            if (TimeManager.Instance.curTimeState == TimeState.pause || BattleManager.Instance.CurGameState == GameState.GameOver)
            {
                yield return null;
                continue;
            }

            StartCoroutine(SpawnAtPortal());

            delay = UnityEngine.Random.Range(1f, 2f);
            timer = 0f;
            while (timer < delay)
            {
                timer += TimeManager.Instance.DeltaTime;
                gameTime += TimeManager.Instance.DeltaTime;
                yield return null;
            }
            gameTime += TimeManager.Instance.DeltaTime;
            yield return null;
        }

        float restTimer = 0f;
        while (restTimer < restTime)
        {
            restTimer += TimeManager.Instance.DeltaTime;
            gameTime += TimeManager.Instance.DeltaTime;
            yield return null;
        }
        
        BattleManager.Instance.GameEnd(true);
    }

    private IEnumerator SpawnAtPortal()
    {
        float portalDelay = 0.2f;
        float delay;

        int n = UnityEngine.Random.Range(1, maxEnemyPerPortal);

        Vector3 portalPosition = GetPortalPos();
        GameObject portal = Instantiate(portalEffect, portalPosition, quaternion.identity);

        float timer = 0f;

        while (timer < portalDelay)
        {
            timer += TimeManager.Instance.DeltaTime;
            yield return null;
        }

        for (int i = 0; i < n; ++i)
        {
            int num = UnityEngine.Random.Range(0, pool.GetEnemyPrefabCount);
            GameObject enemy = pool.GetEnemy(num);
            enemy.transform.position = new Vector3(portalPosition.x + i * 0.3f, portalPosition.y, 0f);
            BattleManager.Instance.AddEnemy(enemy);

            delay = UnityEngine.Random.Range(0.3f, 0.7f);
            timer = 0f;
            while (timer < delay)
            {
                timer += TimeManager.Instance.DeltaTime;
                yield return null;
            }
        }

        timer = 0f;
        while (timer < portalDelay)
        {
            timer += TimeManager.Instance.DeltaTime;
            yield return null;
        }

        Destroy(portal);
    }
}
