using UnityEngine;
using UnityEditor;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System; // 리플렉션 사용

public class GoogleSheetDataImporter : EditorWindow
{
    // [중요!] 여기에 웹에 게시한 CSV 링크를 입력하세요.
    // 링크가 "https/..."로 시작해야 합니다.
    private string statTypesURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vR2Y3ST5T0m_QDu_q_GvwGpRVJC8up50rWAPDm0quJutqbVuTdsvCl5H3vxpDWcpe773Ts5L04PW2Du/pub?gid=0&single=true&output=csv";
    private string baseStatLevelsURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vR2Y3ST5T0m_QDu_q_GvwGpRVJC8up50rWAPDm0quJutqbVuTdsvCl5H3vxpDWcpe773Ts5L04PW2Du/pub?gid=1291589338&single=true&output=csv";
    private string runeDefinitionsURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vR2Y3ST5T0m_QDu_q_GvwGpRVJC8up50rWAPDm0quJutqbVuTdsvCl5H3vxpDWcpe773Ts5L04PW2Du/pub?gid=244091673&single=true&output=csv";

    // 데이터 저장 경로 (반드시 Resources 폴더 하위여야 런타임 로드 가능)
    private const string STAT_TYPE_PATH = "Assets/Resources/GameData/StatTypes";
    private const string RUNE_DEF_PATH = "Assets/Resources/GameData/Runes";
    private const string LEVEL_TABLE_PATH = "Assets/Resources/GameData/BaseStatLevelTable.asset";

    // 캐시: 룬 파싱 시 StatTypeDefinition을 빠르게 찾기 위함
    private Dictionary<string, StatData> _statTypeCache = new Dictionary<string, StatData>();

    [MenuItem("My Tools/Game Data Importer")]
    public static void ShowWindow()
    {
        GetWindow<GoogleSheetDataImporter>("Game Data Importer");
    }

    private void OnGUI()
    {
        GUILayout.Label("Google Sheet Data Importer", EditorStyles.boldLabel);
        
        EditorGUILayout.HelpBox("각 시트의 '웹에 게시(CSV)' URL을 입력하세요.", MessageType.Info);

        statTypesURL = EditorGUILayout.TextField("StatTypes URL", statTypesURL);
        baseStatLevelsURL = EditorGUILayout.TextField("BaseStatLevels URL", baseStatLevelsURL);
        runeDefinitionsURL = EditorGUILayout.TextField("RuneDefinitions URL", runeDefinitionsURL);

        if (GUILayout.Button("Import All Data", GUILayout.Height(40)))
        {
            if (!EditorUtility.DisplayDialog("Confirm Import", 
                "모든 데이터를 시트에서 가져옵니다.\n" +
                "기존 SO 에셋을 덮어씁니다. 계속하시겠습니까?", "Import", "Cancel"))
            {
                return;
            }
            
            _statTypeCache.Clear();
            ImportDataAsync();
        }
    }

    private async void ImportDataAsync()
    {
        Debug.Log("Game Data Import... STARTED");
        try
        {
            // 1. StatTypes 임포트 (가장 먼저 실행되어야 캐시가 채워짐)
            EnsureDirectoryExists(STAT_TYPE_PATH);
            string csvData = await DownloadCSV(statTypesURL);
            ParseAndCreateStatTypes(csvData);

            // 2. BaseStatLevels 임포트
            EnsureDirectoryExists(Path.GetDirectoryName(LEVEL_TABLE_PATH));
            csvData = await DownloadCSV(baseStatLevelsURL);
            ParseAndCreateBaseStatLevels(csvData);

            // 3. RuneDefinitions 임포트 (StatTypes 캐시 필요)
            EnsureDirectoryExists(RUNE_DEF_PATH);
            csvData = await DownloadCSV(runeDefinitionsURL);
            ParseAndCreateRuneDefinitions(csvData);

            Debug.Log("Game Data Import... SUCCESS!");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Game Data Import... FAILED: {e.Message}");
        }
        finally
        {
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            _statTypeCache.Clear();
        }
    }

    private async Task<string> DownloadCSV(string url)
    {
        using (HttpClient client = new HttpClient())
        {
            return await client.GetStringAsync(url);
        }
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    // --- 1. StatTypes 파서 ---
    private void ParseAndCreateStatTypes(string csvData)
    {
        var lines = csvData.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return; // 헤더 + 최소 1줄 데이터

        var headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            var data = CreateRowDictionary(headers, values);

            string typeID = data["TypeID"].Trim();
            string path = $"{STAT_TYPE_PATH}/StatType_{typeID}.asset";

            StatData statType = AssetDatabase.LoadAssetAtPath<StatData>(path);
            if (statType == null)
            {
                statType = ScriptableObject.CreateInstance<StatData>();
                AssetDatabase.CreateAsset(statType, path);
            }

            statType.type = (StatType)Enum.Parse(typeof(StatType), typeID);
            statType.statName = data["DisplayName"].Trim();
            statType.Description = data["Description"].Trim();
            statType.IconName = data["IconName"].Trim();

            EditorUtility.SetDirty(statType);
            _statTypeCache[typeID] = statType; // 캐시에 저장
        }
        Debug.Log($"[Importer] Parsed {lines.Length - 1} StatTypes.");
    }

    // --- 2. BaseStatLevels 파서 ---
    private void ParseAndCreateBaseStatLevels(string csvData)
    {
        var lines = csvData.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        var headers = lines[0].Split(',');
        
        // LevelData의 필드 정보 캐시 (리플렉션용)
        var fields = typeof(BaseStatLevelTable.LevelData).GetFields(BindingFlags.Public | BindingFlags.Instance);
        var fieldMap = new Dictionary<string, FieldInfo>();
        foreach (var f in fields)
        {
            fieldMap[f.Name.ToUpper()] = f; // 대소문자 무시 매칭
        }

        BaseStatLevelTable table = AssetDatabase.LoadAssetAtPath<BaseStatLevelTable>(LEVEL_TABLE_PATH);
        if (table == null)
        {
            table = ScriptableObject.CreateInstance<BaseStatLevelTable>();
            AssetDatabase.CreateAsset(table, LEVEL_TABLE_PATH);
        }
        
        table.allLevelData.Clear();

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            var data = CreateRowDictionary(headers, values);
            var levelData = new BaseStatLevelTable.LevelData();

            foreach (var header in headers)
            {
                string headerTrimmed = header.Trim().ToUpper();
                if (fieldMap.TryGetValue(headerTrimmed, out FieldInfo field))
                {
                    string valueStr = data[header].Trim();
                    try
                    {
                        // 필드 타입에 맞게 파싱
                        if (field.FieldType == typeof(int))
                        {
                            field.SetValue(levelData, int.Parse(valueStr));
                        }
                        else if (field.FieldType == typeof(float))
                        {
                            field.SetValue(levelData, float.Parse(valueStr));
                        }
                        // (필요시 다른 타입도 추가)
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogWarning($"[Importer] Parse error for '{header}': '{valueStr}'. {e.Message}");
                    }
                }
            }
            table.allLevelData.Add(levelData);
        }
        
        EditorUtility.SetDirty(table);
        Debug.Log($"[Importer] Parsed {lines.Length - 1} Levels.");
    }

    // --- 3. RuneDefinitions 파서 ---
    private void ParseAndCreateRuneDefinitions(string csvData)
    {
        if (_statTypeCache.Count == 0)
        {
            Debug.LogError("[Importer] StatType cache is empty. Run StatType import first.");
            // 실제 사용 시: _statTypeCache를 강제로 채우는 로직 추가
            // LoadAllStatTypesIntoCache();
            return;
        }

        var lines = csvData.Split(new[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length < 2) return;

        var headers = lines[0].Split(',');

        for (int i = 1; i < lines.Length; i++)
        {
            var values = lines[i].Split(',');
            var data = CreateRowDictionary(headers, values);

            string runeID = data["RuneID"].Trim();
            string path = $"{RUNE_DEF_PATH}/Rune_{runeID}.asset";

            RuneDefinition rune = AssetDatabase.LoadAssetAtPath<RuneDefinition>(path);
            if (rune == null)
            {
                rune = ScriptableObject.CreateInstance<RuneDefinition>();
                AssetDatabase.CreateAsset(rune, path);
            }

            rune.RuneID = runeID;
            rune.DisplayName = data["DisplayName"].Trim();
            rune.IconName = data["IconName"].Trim();
            rune.Value = float.Parse(data["Value"].Trim());

            // StatType 연결
            string targetStatID = data["TargetStatTypeID"].Trim();
            if (_statTypeCache.TryGetValue(targetStatID, out StatData statType))
            {
                rune.TargetStatType = statType;
            }
            else
            {
                Debug.LogWarning($"[Importer] Could not find StatType '{targetStatID}' for Rune '{runeID}'.");
            }

            // Enum 파싱
            string modTypeStr = data["ModType"].Trim();
            try
            {
                rune.ModType = (StatModType)System.Enum.Parse(typeof(StatModType), modTypeStr, true);
            }
            catch
            {
                Debug.LogWarning($"[Importer] Invalid ModType '{modTypeStr}' for Rune '{runeID}'.");
            }

            EditorUtility.SetDirty(rune);
        }
        Debug.Log($"[Importer] Parsed {lines.Length - 1} Runes.");
    }


    // CSV 파싱 헬퍼
    private Dictionary<string, string> CreateRowDictionary(string[] headers, string[] values)
    {
        var dict = new Dictionary<string, string>();
        for (int i = 0; i < headers.Length; i++)
        {
            if (i < values.Length)
            {
                dict[headers[i].Trim()] = values[i].Trim();
            }
        }
        return dict;
    }
}