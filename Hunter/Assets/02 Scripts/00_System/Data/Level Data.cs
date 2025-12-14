using System;
using UnityEngine;
using System.IO;

[CreateAssetMenu(fileName = "NewLevelData", menuName = "Data/LevelData")]
public class LevelData : ScriptableObject
{
    public int stageNum = 0;

    [Header("1. 스테이지 규칙")]
    public StageMissionData mission;

    [Header("2. 플레이어 설정")]
    public PlayerData player;

    [Header("3. 적 설정 (이동 & 스탯)")]
    public EnemyBaseData enemyBase;

    [Header("4. 적 행동 패턴")]
    public PatternData enemyPattern;

    [Header("5. 투사체 & 조작 (슈팅)")]
    public ProjectileData projectile;

    [Header("6. 카메라 설정")]
    public CameraData cameraSetting;

    [Header("7. 카메라 셰이킹")]
    public CameraShaking cameraShaking;

    private string GetPath(string fileName)
    {
        // 입력받은 이름에 .json이 없으면 붙여줌
        if (!fileName.EndsWith(".json"))
        {
            fileName += ".json";
        }
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    public void SaveToFile(string fileName)
    {
        string json = JsonUtility.ToJson(this, true);
        string path = GetPath(fileName);
        File.WriteAllText(path, json);

        Debug.Log($"[저장 완료] 파일명: {fileName}\n경로: {path}");
    }

    public void LoadFromFile(string fileName)
    {
        string path = GetPath(fileName);

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            JsonUtility.FromJsonOverwrite(json, this); 
            Debug.Log($"[로드 완료] 데이터가 {fileName} 설정으로 변경되었습니다.");
        }
        else
        {
            Debug.LogError($"파일을 찾을 수 없습니다: {path}");
        }
    }
}

[Serializable]
public class StageMissionData
{
    public StageType stageType = StageType.time_limit;
    public int timeLimit = 40;
    public int countLimit = 20;
    public int weakPointCount = 3;
}

[Serializable]
public class PlayerData
{
    public float scale = 1.0f;
    public float speed = 1.0f;
    public Vector2 startViewportPos = new Vector2(0.36f, 0.19f);
}

[Serializable]
public class EnemyBaseData
{
    [Header("기본 스탯")]
    public float maxHealth = 100f;
    public float scale = 1f;
    public float speed = 1.5f;
    public Vector2 startViewportPos = new Vector2(0.8f, 0.17f);

    [Header("이동 AI (추적/휴식)")]
    public float accelerationTime = 0.5f;
    public float startAccelMultiplier = 0.2f;
    public float reactionDelay = 0.5f;
    public float chaseDuration = 3f;
    public float restDuration = 2f;

    [Header("거리 유지")]
    public float stopWorldDistance = -1f;
    public float attackWorldDistance = -1f;

    [Header("공격 속도")]
    public float attackSpeedMultiplier = 5f;
}

[Serializable]
public class CameraData
{
    public float yPos = 0f;
    public float size = 6f;
    public float followSpeed = 3f;
    public float smoothStopDuration = 0.5f;
    public float viewportThresholdX = 0.3f;
}

[Serializable]
public class ProjectileData
{
    [Header("전투 스탯")]
    public float damage = 10f;
    public float hatDamage = 10f; 
    public float lifeTime = 5f;
    public float speedMultiplier = 2.0f;

    [Header("조작감 (Control Feel)")]
    public float sensitivity = 1.0f; 
    public float aimSmoothing = 0.1f; 
    public float minDragDistance;
    public float maxDragRadius = 300f;
    public float inputDeadzone = 10f;
    public float minAngle = -10f;
    public float maxAngle = 85f;

    [Header("물리 (Physics)")]
    public float minPower = 5f;
    public float maxPower = 25f;
    public float powerMultiplier = 10f;
    public float gravity = -25f;
    public float fallGravityMultiplier = 2.5f;
    public float initialSpeedBoost = 1.2f;

    [Header("비주얼 (Visual & Effect)")]
    public int trajectoryDotCount = 30;
    public float dotSpacing = 0.05f;
    public float minAlpha = 0.2f;
    public float flowSpeed = 2f;

    public Vector2 punchScaleStrength = new Vector2(0.3f, 0.3f);
    public float punchDuration = 0.3f;
    public float flashDuration = 0.15f;
}
[Serializable]
public class PatternData
{
    public StateWeight[] stateWeights = new StateWeight[]
    {
        new StateWeight { state = EnemyState.DEFENSE_LOW, isUse = false },
        new StateWeight { state = EnemyState.DEFENSE_HIGH, isUse = false },
        new StateWeight { state = EnemyState.ATTACK, isUse = true }
    };

    public int dusting_arrow_num = 5;
    public float decision_delay = 1f;
    public bool canDoubleAttack = false;
    public bool canDoubleDefense = false;
    public bool canAttackAfterMove = false;

    public float attack_cool_time = 30f;
    public float defense_low_cool_time = 30f;
    public float defense_high_cool_time = 30f;
}

[Serializable]
public class CameraShaking
{
    public float duration = 0.2f;
    public float magnitude = 0.5f;
}

[Serializable]
public class StateWeight
{
    public EnemyState state;
    public bool isUse;
}

public enum StageType
{
    time_limit = 0,
    count_limit = 1,
    weak_point = 2,
}