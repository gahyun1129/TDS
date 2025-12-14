using UnityEngine;
using System.Collections.Generic; // HashSet 사용을 위해 추가

public class InfiniteBackground : MonoBehaviour
{
    [Header("Background Layers")]
    [SerializeField] private BackgroundLayer[] layers;

    [Header("Player Reference")]
    [SerializeField] private BearController player;

    private Camera mainCamera;
    private LevelData levelData;

    // [NEW] 튜토리얼 모드 관련 변수
    private bool isTutorialMode = false;
    private float tutorialSpeed = 0f;
    private HashSet<int> tutorialTargetLayerIndices = new HashSet<int>();

    void Start()
    {
        mainCamera = Camera.main;

        // GameData 가져오기 (싱글톤 null 체크 추가 권장)
        if (InGameManager.Instance != null)
            levelData = InGameManager.Instance.currentLevelData;

        // 플레이어 자동 찾기
        if (player == null)
        {
            player = FindObjectOfType<BearController>();
        }

        // 각 레이어의 너비 계산
        InitializeLayers();
    }

    void Update()
    {
        // [NEW] 튜토리얼 모드일 경우 우선 처리
        if (isTutorialMode)
        {
            ScrollTutorialLayers();
        }
        // 기존 로직: 튜토리얼이 아니고 플레이어가 움직일 때만
        else if (player != null && player.IsMoving())
        {
            ScrollLayers();
        }
    }

    // ========================================================================
    // [NEW] 튜토리얼용 함수 추가
    // ========================================================================

    /// <summary>
    /// 튜토리얼 스크롤 시작
    /// </summary>
    /// <param name="layerIndices">움직이고 싶은 레이어의 번호들 (예: {0, 2})</param>
    /// <param name="speed">이동 속도 (양수면 x축 +방향)</param>
    public void StartTutorialScroll(int[] layerIndices, float speed)
    {
        isTutorialMode = true;
        tutorialSpeed = speed;

        // 움직일 레이어 목록 등록
        tutorialTargetLayerIndices.Clear();
        if (layerIndices != null)
        {
            foreach (int index in layerIndices)
            {
                if (index >= 0 && index < layers.Length)
                {
                    tutorialTargetLayerIndices.Add(index);
                }
            }
        }
    }

    /// <summary>
    /// 튜토리얼 모드 종료 (기본값/플레이어 이동 모드로 초기화)
    /// </summary>
    public void StopTutorialScroll()
    {
        isTutorialMode = false;
        tutorialSpeed = 0f;
        tutorialTargetLayerIndices.Clear();
    }

    // 튜토리얼 전용 스크롤 로직
    private void ScrollTutorialLayers()
    {
        for (int i = 0; i < layers.Length; i++)
        {
            // 선택된 레이어만 움직임
            if (tutorialTargetLayerIndices.Contains(i))
            {
                BackgroundLayer layer = layers[i];
                if (layer.object1 == null || layer.object2 == null || layer.object3 == null) continue;

                // 요청하신 대로 x축 + 방향으로 이동
                // (배경 레이어 자체의 speedMultiplier도 적용할지 여부는 선택사항이나, 
                // 보통 튜토리얼 강제 이동은 입력한 speed로 통일하는 게 자연스러워서 multiplier는 뺐습니다.
                // 필요하면 * layer.speedMultiplier 를 추가하세요)
                float moveAmount = tutorialSpeed * Time.deltaTime;

                layer.object1.position += Vector3.right * moveAmount;
                layer.object2.position += Vector3.right * moveAmount;
                layer.object3.position += Vector3.right * moveAmount;

                // 무한 루프 처리는 기존 로직 재활용
                CheckAndRepositionLayer(layer);
            }
        }
    }
    // ========================================================================

    // 레이어 초기화 (너비 계산 및 위치 설정)
    private void InitializeLayers()
    {
        foreach (var layer in layers)
        {
            if (layer.object1 == null || layer.object2 == null || layer.object3 == null) continue;

            // 레이어 너비 설정 (수동 설정이 없으면 자동 계산)
            if (layer.layerWidth <= 0f)
            {
                // 자동 계산: 모든 자식 SpriteRenderer의 범위를 계산
                Renderer[] renderers = layer.object1.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    // 모든 렌더러의 바운드를 합쳐서 전체 너비 계산
                    Bounds combinedBounds = renderers[0].bounds;
                    for (int i = 1; i < renderers.Length; i++)
                    {
                        combinedBounds.Encapsulate(renderers[i].bounds);
                    }
                    layer.layerWidth = combinedBounds.size.x;
                }
                else
                {
                    Debug.LogWarning($"Renderer not found on {layer.object1.name}, 기본값 10 사용");
                    layer.layerWidth = 10f; // 기본값
                }
            }

            layer.isWidthCalculated = true;

            // 초기 배치
            layer.object2.position = new Vector3(
                layer.object1.position.x - layer.layerWidth,
                layer.object1.position.y,
                layer.object1.position.z
            );

            layer.object3.position = new Vector3(
                layer.object1.position.x + layer.layerWidth,
                layer.object1.position.y,
                layer.object1.position.z
            );
        }
    }

    // 기존 플레이어 기반 스크롤
    private void ScrollLayers()
    {
        if (levelData == null) return;

        foreach (var layer in layers)
        {
            if (layer.object1 == null || layer.object2 == null || layer.object3 == null) continue;

            float moveAmount = levelData.player.speed * layer.speedMultiplier * Time.deltaTime;

            layer.object1.position += Vector3.right * moveAmount;
            layer.object2.position += Vector3.right * moveAmount;
            layer.object3.position += Vector3.right * moveAmount;

            CheckAndRepositionLayer(layer);
        }
    }

    // 레이어가 화면 밖으로 나가면 재배치
    private void CheckAndRepositionLayer(BackgroundLayer layer)
    {
        float cameraRightEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 0.5f, 0)).x;

        Transform leftmost = layer.object1;
        if (layer.object2.position.x < leftmost.position.x) leftmost = layer.object2;
        if (layer.object3.position.x < leftmost.position.x) leftmost = layer.object3;

        // object1 체크
        if (layer.object1.position.x - layer.layerWidth / 2 > cameraRightEdge)
        {
            layer.object1.position = new Vector3(leftmost.position.x - layer.layerWidth, layer.object1.position.y, layer.object1.position.z);
        }

        // object2 체크
        if (layer.object2.position.x - layer.layerWidth / 2 > cameraRightEdge)
        {
            leftmost = GetLeftmostObject(layer); // 위치가 바뀌었을 수 있으므로 다시 찾음 (안전장치)
            layer.object2.position = new Vector3(leftmost.position.x - layer.layerWidth, layer.object2.position.y, layer.object2.position.z);
        }

        // object3 체크
        if (layer.object3.position.x - layer.layerWidth / 2 > cameraRightEdge)
        {
            leftmost = GetLeftmostObject(layer);
            layer.object3.position = new Vector3(leftmost.position.x - layer.layerWidth, layer.object3.position.y, layer.object3.position.z);
        }
    }

    // [Helper] 가장 왼쪽 오브젝트 찾는 로직이 중복되어 함수로 분리 (선택 사항)
    private Transform GetLeftmostObject(BackgroundLayer layer)
    {
        Transform leftmost = layer.object1;
        if (layer.object2.position.x < leftmost.position.x) leftmost = layer.object2;
        if (layer.object3.position.x < leftmost.position.x) leftmost = layer.object3;
        return leftmost;
    }

    public void SetLayerSpeedMultiplier(int layerIndex, float multiplier)
    {
        if (layerIndex >= 0 && layerIndex < layers.Length)
        {
            layers[layerIndex].speedMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
        }
    }

    public float GetLayerSpeedMultiplier(int layerIndex)
    {
        if (layerIndex >= 0 && layerIndex < layers.Length)
            return layers[layerIndex].speedMultiplier;
        return 0f;
    }

    public int GetLayerCount()
    {
        return layers != null ? layers.Length : 0;
    }

    public void SetGlobalSpeedMultiplier(float multiplier)
    {
        foreach (var layer in layers)
        {
            layer.speedMultiplier = Mathf.Clamp(multiplier, 0f, 2f);
        }
    }
}

[System.Serializable]
public class BackgroundLayer
{
    [Header("Layer Objects")]
    public Transform object1;
    public Transform object2;
    public Transform object3;

    [Header("Layer Settings")]
    [Range(0f, 2f)]
    public float speedMultiplier = 1f;

    public float layerWidth = 0f;

    [HideInInspector]
    public bool isWidthCalculated = false;
}