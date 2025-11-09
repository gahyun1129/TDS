using System;
using System.Collections.Generic;
using UnityEngine;

public enum GameState
{
    Ready,
    Playing,
    GameOver
}

/// <summary>
/// 전투 씬의 모든 데이터를 관리합니다.
/// 현재 게임 상태(상태, 스테이지 수, 포인트), 플레이어, 적/아이템 리스트
/// </summary>
public class BattleManager : MonoBehaviour
{
    /*
        현재 게임 상태
        플레이어
        적 List 관리
        파편 List 관리
    */

    public static BattleManager Instance { get; private set; }

    [Header("현재 게임 상태")]
    [SerializeField] private GameState currentGameState;
    [SerializeField] private int currentStage;
    [SerializeField] private int currentPoint;

    [Header("플레이어")]
    [SerializeField] private GameObject player;

    [Header("적")]
    [SerializeField] private List<GameObject> enemies;

    [Header("파편")]
    [SerializeField] private List<GameObject> runes;

    [Header("파편 관련")]
    [SerializeField] private int maxRuneNum = 2;
    [SerializeField] [Range(0f, 1f)] private float runeSpawnChance = 0.5f;
    [SerializeField] private int maxRunesPerWave = 5;
    [SerializeField] private float runeSpawnInterval = 1.0f;

    private int runesSpawnedThisWave = 0;
    private float lastCogSpawnTime = 0f;

    public event Action<int> OnGameReady;
    public event Action OnGameStart;
    public event Action<bool> OnGameEnd;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }

        currentGameState = GameState.Ready;
    }

    void Start()
    {
        GameReady();
    }

    public GameState CurGameState => currentGameState;

    public void GameReady()
    {
        currentGameState = GameState.Ready;
        GameManager.Instance.GoToNextStage();
        OnGameReady?.Invoke(currentStage);
    }

    public void GameStart()
    {
        currentGameState = GameState.Playing;
        OnGameStart?.Invoke();
    }

    public void GameEnd(bool _isWin)
    {
        currentGameState = GameState.GameOver;

        foreach (var e in enemies)
        {
            e.GetComponent<EnemyAutoAI>().ExplosionEnemy();
        }

        OnGameEnd?.Invoke(_isWin);
    }

    public GameObject Player => player;

    public List<GameObject> Enemis => enemies;

    public void AddEnemy(GameObject enemy)
    {
        Enemis.Add(enemy);
    }

    public void DeleteEnemy(GameObject enemy)
    {
        Enemis.Remove(enemy);
        ObjectPoolManager.Instance.ReturnToPool(enemy);
    }

    public void ClearEnemis()
    {
        foreach (var e in Enemis)
        {
            ObjectPoolManager.Instance.ReturnToPool(e);
        }
        Enemis.Clear();
    }

    public GameObject GetFirstEnemy()
    {
        if (enemies.Count != 0)
        {
            return enemies[0];
        }
        return null;
    }

    public List<GameObject> Runes => runes;

    public void TrySpawnRune(Vector3 position)
    {
        if (runes.Count >= maxRuneNum) return;
        if (runesSpawnedThisWave >= maxRunesPerWave) return;
        if (Time.time - lastCogSpawnTime < runeSpawnInterval) return;
        if (UnityEngine.Random.value > runeSpawnChance) return;

        GameObject rune = ObjectPoolManager.Instance.GetCog();
        RuneData runeData = GameDataManager.Instance.GetSkillDataBase().GetRandomCog();
        rune.GetComponent<Rune>().SetRuneData(runeData);
        rune.transform.position = position;

        runes.Add(rune);
        runesSpawnedThisWave++;
        lastCogSpawnTime = Time.time;
    }

    public void EquippedCog(GameObject cog)
    {
        runes.Remove(cog);
        ObjectPoolManager.Instance.ReturnToPool(cog);
    }

    public void ClearCogs()
    {
        foreach (var e in Runes)
        {
            ObjectPoolManager.Instance.ReturnToPool(e);
        }
        Runes.Clear();
    }

    public void RemoveCog(GameObject cog)
    {
        ObjectPoolManager.Instance.ReturnToPool(cog);
        runes.Remove(cog);
    }


    public void ResetWaveStats()
    {
        runesSpawnedThisWave = 0;
        lastCogSpawnTime = 0f;
    }

    public ObjectPoolManager Pool => ObjectPoolManager.Instance;
}
