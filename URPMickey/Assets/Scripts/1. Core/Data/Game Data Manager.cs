using System.IO;
using UnityEngine;

/// <summary>
/// 게임의 모든 데이터를 관리하는 싱글톤 매니저입니다.
/// (영구 데이터 저장/로드, 핵심 데이터베이스(SO) 참조)
/// 게임 내에서 영구적으로 업그레이드 시, 해당 스크립트에서 저장/로드 합니다.
/// 영구적인 업그레이드는 해당 스크립트에 접근해야 가능합니다.
/// </summary>
public class GameDataManager : MonoBehaviour
{
    public static GameDataManager Instance { get; private set; }

    [Header("게임 기본 데이터")]
    [SerializeField] private GameDataDataBase database;
    [SerializeField] private SkillDataDataBase skillDataBase;

    private PlayerPersistentData playerData;
    private string _savePath;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        DontDestroyOnLoad(gameObject);

        //_savePath = Path.Combine(Application.persistentDataPath, "save.json");
        _savePath = Path.Combine( Application.dataPath, "save.json");


        LoadData();

        database.Initialize();
    }

    public GameDataDataBase GetDataBase()
    {
        if (database == null)
        {
            Debug.LogError("[GameDataManager] _database(SO)가 인스펙터에 연결되지 않았습니다!");
        }
        return database;
    }

    public PlayerPersistentData GetPlayerData()
    {
        if (playerData == null)
        {
            Debug.LogWarning("[GameDataManager] GetPlayerData() 호출 시 _playerData가 null입니다. (LoadData가 아직 실행되지 않았거나 실패함)");
            LoadData(); // 비상시 재로드 시도
        }
        return playerData;
    }

    public SkillDataDataBase GetSkillDataBase()
    {
        if (skillDataBase == null)
        {
            Debug.LogError("[GameDataManager] skillDataBase(SO)가 인스펙터에 연결되지 않았습니다!");
        }
        return skillDataBase;
    }

    /// <summary>
    /// [핵심] JSON 파일에서 플레이어 데이터를 로드합니다.
    /// </summary>
    public void LoadData()
    {
        try
        {
            if (File.Exists(_savePath))
            {
                string json = File.ReadAllText(_savePath);
                playerData = JsonUtility.FromJson<PlayerPersistentData>(json);
                
                Debug.Log($"[GameDataManager] 데이터 로드 성공. ({_savePath})");
            }
            else
            {
                Debug.Log("[GameDataManager] 저장 파일이 없어 새 데이터를 생성합니다.");
                playerData = new PlayerPersistentData();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameDataManager] 데이터 로드 실패: {ex.Message}");
            playerData = new PlayerPersistentData();
        }

        if (playerData.statLevels == null)
        {
            playerData.statLevels = new System.Collections.Generic.List<StatLevelEntry>();
        }
    }

    /// <summary>
    /// [핵심] 현재 플레이어 데이터를 JSON 파일로 저장합니다.
    /// </summary>
    public void SaveData()
    {
        if (playerData == null)
        {
            Debug.LogError("[GameDataManager] 저장할 _playerData가 null입니다.");
            return;
        }

        try
        {
            string json = JsonUtility.ToJson(playerData, true);
            
            File.WriteAllText(_savePath, json);
            
            Debug.Log($"[GameDataManager] 데이터 저장 성공. ({_savePath})");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[GameDataManager] 데이터 저장 실패: {ex.Message}");
        }
    }

    public void UpgradeStat(StatType type, int level)
    {
        if (playerData == null) return;

        playerData.SetStatLevel(type, level);

        SaveData();

        Debug.Log($"[GameDataManager] 스탯 업그레이드: {type}, 새 레벨: {level}");
    }

    public void UpdateHighestWave(int _wave)
    {
        if (playerData == null) return;

        playerData.highestWave = _wave;

        SaveData();

        Debug.Log($"[GameDataManager] 최고 기록 갱신: {_wave}");
    }

    public void UpdateCurrency(int _value)
    {
        if (playerData == null) return;

        playerData.currency = _value;

        SaveData();

        Debug.Log($"[GameDataManager] 재화 변경: {_value}");
    }
    
}
