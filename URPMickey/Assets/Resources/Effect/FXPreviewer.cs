using UnityEngine;
using System.Collections.Generic;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditorInternal;

/// <summary>
/// FX 프리팹들을 월드/UI로 분류해 미리보기하는 툴 (플레이 모드에서 사용)
/// ※ 기능 로직은 그대로, 스타일만 단색(Simple)으로 재정비
/// </summary>
[ExecuteInEditMode]
public class FXPreviewer : MonoBehaviour
{
    [Header("UI 설정")]
    [Range(10, 40)]
    public int fontSize = 14;

    [Header("카메라 제어 설정")]
    public float minCameraZoom = 1f;
    public float maxCameraZoom = 20f;

    [Header("미리보기 보정")]
    [Tooltip("재생 시 ParticleSystem의 Start Rotation을 0도로 강제(프리팹 원본에는 영향 X)")]
    public bool forceZeroStartRotation = true;

    [Tooltip("재생 시 모든 Transform의 로컬 회전을 0도로 강제(RectTransform 제외)")]
    public bool forceZeroTransformRotation = false;

    // -------------------- [STYLE] 옵션 --------------------
    [Header("버튼/컨트롤 스타일")]
    public bool useSolidButtons = true;

    [Tooltip("리스트 비선택 행 기본 배경색")]
    public Color solidButtonBaseColor = new Color(0.30f, 0.30f, 0.30f, 1f);

    // 상단 탭 색
    public Color tabSelectedBg = Color.white;               // 선택 탭: 유지(흰/검)
    public Color tabSelectedText = Color.black;
    public Color tabUnselectedBg = new Color(0.22f, 0.22f, 0.22f, 1f);
    public Color tabUnselectedText = new Color(0.9f, 0.9f, 0.9f, 1f); // 미선택 탭 텍스트 0.9

    // 동작 버튼(이전/다음/재생)
    public Color actionBtnBg = new Color(0.22f, 0.22f, 0.22f, 1f);
    public Color actionBtnText = new Color(0.9f, 0.9f, 0.9f, 1f);      // 모든 버튼 텍스트 0.9

    // 슬라이더/스크롤바 색
    public Color sliderTrack = new Color(0.25f, 0.25f, 0.25f, 1f);
    public Color sliderThumb = new Color(0.55f, 0.55f, 0.55f, 1f);
    public Color scrollbarTrack = new Color(0.20f, 0.20f, 0.20f, 1f);
    public Color scrollbarThumb = new Color(0.55f, 0.55f, 0.55f, 1f);

    // 그룹 헤더(루트 폴더 라벨)
    public Color headerBg = new Color(0.60f, 0.80f, 1.00f, 1f);  // 하늘색
    public Color headerText = Color.black;

    // 리스트(스크롤 영역) 배경색
    public Color listBg = new Color(0.15f, 0.15f, 0.15f, 1f);

    // 선택행 주황( Color 로 통일해 CS0172 방지 )
    private static readonly Color kSelectedRow = new Color(1f, 0.6470588f, 0f, 1f);

    // -------------------- 내부 데이터 --------------------
    private class FXPrefabInfo
    {
        public GameObject Prefab;
        public string RootFolderName;
        public bool IsUiFx;
    }

    private List<FXPrefabInfo> _allFx;
    private List<FXPrefabInfo> _uiFx;
    private List<FXPrefabInfo> _worldFx;
    private int _currentIndex = 0;
    private GameObject _currentInstance;

    private enum ViewMode { World, UI }
    private ViewMode _viewMode = ViewMode.World;

    // EditorPrefs 키
    private const string PREFS_KEY_RECT_X = "FXPreviewer_RectX";
    private const string PREFS_KEY_RECT_Y = "FXPreviewer_RectY";
    private const string PREFS_KEY_RECT_W = "FXPreviewer_RectW";
    private const string PREFS_KEY_RECT_H = "FXPreviewer_RectH";
    private const string PREFS_KEY_FONT_SIZE = "FXPreviewer_FontSize";
    private const string PREFS_KEY_VIEWMODE = "FXPreviewer_ViewMode";

    // 창 상태/입력
    private Rect _windowRect = new Rect(10, 10, 300, 400);
    private const float ResizeHandleSize = 20f;
    private Vector2 _scrollPosition = Vector2.zero;
    private bool _needsScrollUpdate = false;
    private float _scrollViewHeight = 0f;

    private double _lastEditorUpdateTime = 0;
    private const double EditorUpdateInterval = 1.0 / 60.0;

    private enum ResizeCorner { None, TopLeft, TopRight, BottomLeft, BottomRight }
    private ResizeCorner _activeResizeCorner = ResizeCorner.None;

    // 카메라
    private Camera _mainCamera;
    private float _initialCameraSize;
    private bool _isOrthographic;

    // UI FX 루트
    private Transform _uiFxRoot;

    // ---------- [STYLE] 캐시 ----------
    private GUIStyle _solidButtonStyle;
    private GUIStyle _hSlider, _hSliderThumb, _vSlider, _vSliderThumb;
    private GUIStyle _hScrollbar, _hScrollbarThumb, _vScrollbar, _vScrollbarThumb;
    private GUIStyle _scrollViewBg;
    private GUIStyle _headerStyle;
    private Texture2D _whiteTex;

    // 색 텍스처 캐시
    private readonly Dictionary<Color, Texture2D> _texCache = new Dictionary<Color, Texture2D>();

    // 원래 스킨 보관(복구용)
    private GUIStyle _prevBtn, _prevHSlider, _prevHSliderThumb, _prevVSlider, _prevVSliderThumb;
    private GUIStyle _prevHScroll, _prevHScrollThumb, _prevVScroll, _prevVScrollThumb, _prevScrollBg;

    // ----------------------------------------------------------------

    private void OnEnable()
    {
        if (!Application.isEditor) { Destroy(this); return; }

        LoadSettings();
        LoadFXPrefabs();
        SetupCamera();
        SetupUiFxRoot();

        if (!Application.isPlaying)
            EditorApplication.update += EditorUpdate;
    }

    private void OnDisable()
    {
        EditorApplication.update -= EditorUpdate;
        SaveSettings();

        if (_whiteTex != null) { DestroyImmediate(_whiteTex); _whiteTex = null; }
        _solidButtonStyle = null; _hSlider = _hSliderThumb = _vSlider = _vSliderThumb = null;
        _hScrollbar = _hScrollbarThumb = _vScrollbar = _vScrollbarThumb = null;
        _scrollViewBg = null; _headerStyle = null;

        // 텍스처 캐시 정리
        foreach (var kv in _texCache) if (kv.Value) DestroyImmediate(kv.Value);
        _texCache.Clear();

        if (Application.isEditor && _mainCamera != null && Application.isPlaying)
        {
            if (_isOrthographic) _mainCamera.orthographicSize = _initialCameraSize;
            else _mainCamera.fieldOfView = _initialCameraSize;
        }
    }

    private void LoadSettings()
    {
        float x = EditorPrefs.GetFloat(PREFS_KEY_RECT_X, _windowRect.x);
        float y = EditorPrefs.GetFloat(PREFS_KEY_RECT_Y, _windowRect.y);
        float w = EditorPrefs.GetFloat(PREFS_KEY_RECT_W, _windowRect.width);
        float h = EditorPrefs.GetFloat(PREFS_KEY_RECT_H, _windowRect.height);
        _windowRect = new Rect(x, y, w, h);

        fontSize = EditorPrefs.GetInt(PREFS_KEY_FONT_SIZE, fontSize);
        _viewMode = (ViewMode)EditorPrefs.GetInt(PREFS_KEY_VIEWMODE, (int)ViewMode.World);
    }

    private void SaveSettings()
    {
        EditorPrefs.SetFloat(PREFS_KEY_RECT_X, _windowRect.x);
        EditorPrefs.SetFloat(PREFS_KEY_RECT_Y, _windowRect.y);
        EditorPrefs.SetFloat(PREFS_KEY_RECT_W, _windowRect.width);
        EditorPrefs.SetFloat(PREFS_KEY_RECT_H, _windowRect.height);
        EditorPrefs.SetInt(PREFS_KEY_FONT_SIZE, fontSize);
        EditorPrefs.SetInt(PREFS_KEY_VIEWMODE, (int)_viewMode);
    }

    private void LoadFXPrefabs()
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        _allFx = new List<FXPrefabInfo>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (path.ToLower().Contains("fx"))
            {
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (prefab != null)
                {
                    string[] parts = path.Split('/');
                    string root = (parts.Length > 1) ? parts[1] : "기타";
                    bool isUi = PrefabHasUIParticle(prefab);

                    _allFx.Add(new FXPrefabInfo { Prefab = prefab, RootFolderName = root, IsUiFx = isUi });
                }
            }
        }

        _allFx = _allFx.OrderBy(i => i.RootFolderName).ThenBy(i => i.Prefab.name).ToList();
        _uiFx = _allFx.Where(x => x.IsUiFx).ToList();
        _worldFx = _allFx.Where(x => !x.IsUiFx).ToList();

        Debug.Log($"[FXPreviewer] 전체:{_allFx.Count}  UI:{_uiFx.Count}  World:{_worldFx.Count}");
    }

    private void SetupCamera()
    {
        _mainCamera = Camera.main;
        if (_mainCamera != null)
        {
            _isOrthographic = _mainCamera.orthographic;
            _initialCameraSize = _isOrthographic ? _mainCamera.orthographicSize : _mainCamera.fieldOfView;
        }
    }

    private void SetupUiFxRoot()
    {
        var go = GameObject.Find("FX_UI");
        _uiFxRoot = go != null ? go.transform : null;
    }

    private void Update()
    {
        if (!Application.isPlaying || GetActiveList().Count == 0) return;
        if (Input.GetKeyDown(KeyCode.A)) GoToPreviousAndPlay();
        if (Input.GetKeyDown(KeyCode.D)) GoToNextAndPlay();
        if (Input.GetKeyDown(KeyCode.Space)) PlayCurrent();
    }

    private void EditorUpdate()
    {
        double t = EditorApplication.timeSinceStartup;
        if (t - _lastEditorUpdateTime > EditorUpdateInterval)
        {
            _lastEditorUpdateTime = t;
            InternalEditorUtility.RepaintAllViews();
        }
    }

    private void OnGUI()
    {
        var active = GetActiveList();
        if (active == null || active.Count == 0) { DrawEmptyWindow(); return; }

        _windowRect = GUILayout.Window(0, _windowRect, DrawWindowContent, "FX Previewer");
        HandleGlobalResizeInput();
    }

    private void DrawEmptyWindow()
    {
        _windowRect = GUILayout.Window(0, _windowRect, _ =>
        {
            GUI.skin.label.fontSize = fontSize;
            GUI.skin.button.fontSize = fontSize;
            GUI.skin.window.fontSize = fontSize;
            GUI.skin.toggle.fontSize = fontSize;

            DrawCategoryButtons();
            GUILayout.Space(8);
            GUILayout.Label("해당 분류에 프리팹이 없습니다.");
        }, "FX Previewer");
    }

    // ==================== [STYLE] 유틸/스타일 ====================

    private Texture2D WhiteTex()
    {
        if (_whiteTex == null)
        {
            _whiteTex = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
            _whiteTex.SetPixel(0, 0, Color.white);
            _whiteTex.Apply();
        }
        return _whiteTex;
    }

    private Texture2D GetTex(Color c)
    {
        if (_texCache.TryGetValue(c, out var t) && t) return t;
        var tex = new Texture2D(1, 1) { hideFlags = HideFlags.HideAndDontSave };
        tex.SetPixel(0, 0, c);
        tex.Apply();
        _texCache[c] = tex;
        return tex;
    }

    private GUIStyle SolidButton()
    {
        if (_solidButtonStyle == null) _solidButtonStyle = new GUIStyle(GUI.skin.button);
        _solidButtonStyle.normal.background = WhiteTex();
        _solidButtonStyle.hover.background  = WhiteTex();
        _solidButtonStyle.active.background = WhiteTex();
        _solidButtonStyle.onNormal.background = WhiteTex();
        _solidButtonStyle.onHover.background  = WhiteTex();
        _solidButtonStyle.onActive.background = WhiteTex();
        _solidButtonStyle.fontSize = fontSize; // 항상 최신 fontSize 반영
        return _solidButtonStyle;
    }

    private GUIStyle SolidHSlider()
    {
        if (_hSlider == null) _hSlider = new GUIStyle(GUI.skin.horizontalSlider);
        _hSlider.normal.background = GetTex(sliderTrack);
        _hSlider.hover.background  = _hSlider.normal.background;
        _hSlider.active.background = _hSlider.normal.background;
        _hSlider.fixedHeight = 8; _hSlider.margin = new RectOffset(4,4,6,6);
        return _hSlider;
    }
    private GUIStyle SolidHSliderThumb()
    {
        if (_hSliderThumb == null) _hSliderThumb = new GUIStyle(GUI.skin.horizontalSliderThumb);
        _hSliderThumb.normal.background = GetTex(sliderThumb);
        _hSliderThumb.hover.background  = _hSliderThumb.normal.background;
        _hSliderThumb.active.background = _hSliderThumb.normal.background;
        _hSliderThumb.fixedHeight = 8; _hSliderThumb.fixedWidth = 10;
        return _hSliderThumb;
    }
    private GUIStyle SolidVSlider()
    {
        if (_vSlider == null) _vSlider = new GUIStyle(GUI.skin.verticalSlider);
        _vSlider.normal.background = GetTex(sliderTrack);
        _vSlider.hover.background  = _vSlider.normal.background;
        _vSlider.active.background = _vSlider.normal.background;
        _vSlider.fixedWidth = 8; _vSlider.margin = new RectOffset(6,6,4,4);
        return _vSlider;
    }
    private GUIStyle SolidVSliderThumb()
    {
        if (_vSliderThumb == null) _vSliderThumb = new GUIStyle(GUI.skin.verticalSliderThumb);
        _vSliderThumb.normal.background = GetTex(sliderThumb);
        _vSliderThumb.hover.background  = _vSliderThumb.normal.background;
        _vSliderThumb.active.background = _vSliderThumb.normal.background;
        _vSliderThumb.fixedWidth = 8; _vSliderThumb.fixedHeight = 10;
        return _vSliderThumb;
    }

    private GUIStyle SolidHScrollbar()
    {
        if (_hScrollbar == null) _hScrollbar = new GUIStyle(GUI.skin.horizontalScrollbar);
        _hScrollbar.normal.background = GetTex(scrollbarTrack);
        _hScrollbar.hover.background  = _hScrollbar.normal.background;
        _hScrollbar.active.background = _hScrollbar.normal.background;
        _hScrollbar.fixedHeight = 10;
        return _hScrollbar;
    }
    private GUIStyle SolidHScrollbarThumb()
    {
        if (_hScrollbarThumb == null) _hScrollbarThumb = new GUIStyle(GUI.skin.horizontalScrollbarThumb);
        _hScrollbarThumb.normal.background = GetTex(scrollbarThumb);
        _hScrollbarThumb.hover.background  = _hScrollbarThumb.normal.background;
        _hScrollbarThumb.active.background = _hScrollbarThumb.normal.background;
        _hScrollbarThumb.fixedHeight = 10;
        return _hScrollbarThumb;
    }
    private GUIStyle SolidVScrollbar()
    {
        if (_vScrollbar == null) _vScrollbar = new GUIStyle(GUI.skin.verticalScrollbar);
        _vScrollbar.normal.background = GetTex(scrollbarTrack);
        _vScrollbar.hover.background  = _vScrollbar.normal.background;
        _vScrollbar.active.background = _vScrollbar.normal.background;
        _vScrollbar.fixedWidth = 10;
        return _vScrollbar;
    }
    private GUIStyle SolidVScrollbarThumb()
    {
        if (_vScrollbarThumb == null) _vScrollbarThumb = new GUIStyle(GUI.skin.verticalScrollbarThumb);
        _vScrollbarThumb.normal.background = GetTex(scrollbarThumb);
        _vScrollbarThumb.hover.background  = _vScrollbarThumb.normal.background;
        _vScrollbarThumb.active.background = _vScrollbarThumb.normal.background;
        _vScrollbarThumb.fixedWidth = 10;
        return _vScrollbarThumb;
    }
    private GUIStyle SolidScrollViewBg()
    {
        if (_scrollViewBg == null) _scrollViewBg = new GUIStyle(GUI.skin.scrollView);
        _scrollViewBg.normal.background = GetTex(listBg); // ← 흰색 대신 지정 색 사용
        return _scrollViewBg;
    }

    private GUIStyle HeaderStyle()
    {
        if (_headerStyle == null) _headerStyle = new GUIStyle(GUI.skin.label);
        _headerStyle.alignment = TextAnchor.MiddleLeft;
        _headerStyle.fontStyle = FontStyle.Bold;
        _headerStyle.padding = new RectOffset(6, 6, 3, 3);
        _headerStyle.fontSize = fontSize;
        _headerStyle.normal.textColor = headerText;
        _headerStyle.normal.background = GetTex(headerBg); // 하늘색 배경
        return _headerStyle;
    }

    // 버튼(배경/텍스트 색 지정 + fontSize 반영)
    private bool DrawSolidButton(string label, Color bg, Color text, params GUILayoutOption[] opts)
    {
        var style = SolidButton();
        // 텍스트 컬러 모든 상태에 적용
        var prevN = style.normal.textColor;   var prevH = style.hover.textColor;   var prevA = style.active.textColor;
        var prevON = style.onNormal.textColor; var prevOH = style.onHover.textColor; var prevOA = style.onActive.textColor;

        style.normal.textColor = style.hover.textColor = style.active.textColor =
        style.onNormal.textColor = style.onHover.textColor = style.onActive.textColor = text;

        var prevBG = GUI.backgroundColor;
        GUI.backgroundColor = bg;
        bool pressed = GUILayout.Button(label, style, opts);
        GUI.backgroundColor = prevBG;

        // 복구
        style.normal.textColor = prevN; style.hover.textColor = prevH; style.active.textColor = prevA;
        style.onNormal.textColor = prevON; style.onHover.textColor = prevOH; style.onActive.textColor = prevOA;

        return pressed;
    }

    private void PushSolidSkin()
    {
        if (!useSolidButtons) return;

        _prevBtn = GUI.skin.button;
        _prevHSlider = GUI.skin.horizontalSlider;
        _prevHSliderThumb = GUI.skin.horizontalSliderThumb;
        _prevVSlider = GUI.skin.verticalSlider;
        _prevVSliderThumb = GUI.skin.verticalSliderThumb;
        _prevHScroll = GUI.skin.horizontalScrollbar;
        _prevHScrollThumb = GUI.skin.horizontalScrollbarThumb;
        _prevVScroll = GUI.skin.verticalScrollbar;
        _prevVScrollThumb = GUI.skin.verticalScrollbarThumb;
        _prevScrollBg = GUI.skin.scrollView;

        GUI.skin.button = SolidButton();
        GUI.skin.horizontalSlider = SolidHSlider();
        GUI.skin.horizontalSliderThumb = SolidHSliderThumb();
        GUI.skin.verticalSlider = SolidVSlider();
        GUI.skin.verticalSliderThumb = SolidVSliderThumb();
        GUI.skin.horizontalScrollbar = SolidHScrollbar();
        GUI.skin.horizontalScrollbarThumb = SolidHScrollbarThumb();
        GUI.skin.verticalScrollbar = SolidVScrollbar();
        GUI.skin.verticalScrollbarThumb = SolidVScrollbarThumb();
        GUI.skin.scrollView = SolidScrollViewBg();

        // 폰트 사이즈 동기화
        GUI.skin.label.fontSize = fontSize;
        GUI.skin.button.fontSize = fontSize;
        GUI.skin.window.fontSize = fontSize;
        GUI.skin.toggle.fontSize = fontSize;
    }

    private void PopSolidSkin()
    {
        if (!useSolidButtons) return;

        GUI.skin.button = _prevBtn;
        GUI.skin.horizontalSlider = _prevHSlider;
        GUI.skin.horizontalSliderThumb = _prevHSliderThumb;
        GUI.skin.verticalSlider = _prevVSlider;
        GUI.skin.verticalSliderThumb = _prevVSliderThumb;
        GUI.skin.horizontalScrollbar = _prevHScroll;
        GUI.skin.horizontalScrollbarThumb = _prevHScrollThumb;
        GUI.skin.verticalScrollbar = _prevVScroll;
        GUI.skin.verticalScrollbarThumb = _prevVScrollThumb;
        GUI.skin.scrollView = _prevScrollBg;
    }

    // ==================== 윈도 콘텐츠 ====================

    private void DrawWindowContent(int windowID)
    {
        // 심플 스킨 적용
        PushSolidSkin();

        var list = GetActiveList();
        if (_currentIndex >= list.Count) _currentIndex = Mathf.Max(0, list.Count - 1);

        // 탭(가운데 정렬)
        var btnStyle = SolidButton();
        var prevAlign = btnStyle.alignment;
        btnStyle.alignment = TextAnchor.MiddleCenter;
        DrawCategoryButtons();
        btnStyle.alignment = prevAlign;

        GUILayout.Space(6);

        // 정보
        GameObject currentPrefab = list[_currentIndex].Prefab;
        GUILayout.Label($"프리팹: {currentPrefab.name}");
        GUILayout.Label($"({_currentIndex + 1} / {list.Count})");

        bool isPlaying = Application.isPlaying;

        // 액션 버튼(가운데 정렬 + 2px 간격)
        var prevAlign2 = btnStyle.alignment;
        btnStyle.alignment = TextAnchor.MiddleCenter;

        GUILayout.BeginHorizontal();
        if (DrawSolidButton("이전 (A)", actionBtnBg, actionBtnText, GUILayout.ExpandWidth(true)))
        { if (isPlaying) GoToPreviousAndPlay(); else GoToPrevious(); }
        GUILayout.Space(2);
        if (DrawSolidButton("다음 (D)", actionBtnBg, actionBtnText, GUILayout.ExpandWidth(true)))
        { if (isPlaying) GoToNextAndPlay(); else GoToNext(); }
        GUILayout.EndHorizontal();

        GUILayout.Space(2);
        var playLabel = isPlaying ? "재생 / 리플레이 (Space)" : "재생 (플레이 중 사용 가능)";
        if (DrawSolidButton(playLabel, actionBtnBg, actionBtnText, GUILayout.ExpandWidth(true)))
        { if (isPlaying) PlayCurrent(); }

        btnStyle.alignment = prevAlign2;

        GUILayout.Space(10);

        // 카메라 슬라이더 (스타일 색 그대로)
        if (_mainCamera != null || !isPlaying)
        {
            string zoomLabel = _isOrthographic ? "카메라 크기" : "카메라 시야각 (FOV)";
            float currentZoom = isPlaying && _mainCamera != null
                ? (_isOrthographic ? _mainCamera.orthographicSize : _mainCamera.fieldOfView)
                : _initialCameraSize;

            GUILayout.Label($"{zoomLabel}: {currentZoom:F2}");

            float newZoom = GUILayout.HorizontalSlider(
                currentZoom, minCameraZoom, maxCameraZoom,
                GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb
            );

            if (isPlaying && _mainCamera != null && Mathf.Abs(newZoom - currentZoom) > 0.01f)
            {
                if (_isOrthographic) _mainCamera.orthographicSize = newZoom;
                else _mainCamera.fieldOfView = newZoom;
            }
        }
        else GUILayout.Label("메인 카메라를 찾을 수 없습니다.");

        GUILayout.Space(10);

        // UI 설정
        GUILayout.Label("UI 설정");
        GUILayout.BeginHorizontal();
        GUILayout.Label($"폰트 크기: {fontSize}", GUILayout.Width(100));
        int newFontSize = (int)GUILayout.HorizontalSlider(
            fontSize, 10, 40, GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb);
        if (newFontSize != fontSize) { fontSize = newFontSize; SaveSettings(); }
        GUILayout.EndHorizontal();

        forceZeroStartRotation = GUILayout.Toggle(forceZeroStartRotation, "Start Rotation 0으로 강제(프리뷰 전용)");
        forceZeroTransformRotation = GUILayout.Toggle(forceZeroTransformRotation, "Transform Rotation 0으로 강제(프리뷰 전용)");

        GUILayout.Space(10);

        // 리스트
        GUILayout.Label("프리팹 목록:");

        float itemHeight = fontSize + GUI.skin.button.padding.vertical + 5f;
        float groupHeaderHeight = fontSize + 10f;

        if (_needsScrollUpdate && _scrollViewHeight > 0 && itemHeight > 0)
        {
            float estimatedY = 0;
            for (int i = 0; i < _currentIndex; i++)
            {
                if (i == 0 || list[i].RootFolderName != list[i - 1].RootFolderName) estimatedY += groupHeaderHeight;
                estimatedY += itemHeight;
            }
            if (estimatedY < _scrollPosition.y) _scrollPosition.y = estimatedY;
            else if (estimatedY + itemHeight > _scrollPosition.y + _scrollViewHeight)
                _scrollPosition.y = estimatedY + itemHeight - _scrollViewHeight;
            _needsScrollUpdate = false;
        }

        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));

        GUIStyle header = HeaderStyle();
        header.fontSize = fontSize;

        for (int i = 0; i < list.Count; i++)
        {
            if (i == 0 || list[i].RootFolderName != list[i - 1].RootFolderName)
            {
                GUILayout.Space(6);
                GUILayout.Label($"📁 {list[i].RootFolderName}", header); // 하늘색 배경 + 검정 글씨
            }
                   
            // 선택/비선택 배경 + 텍스트 컬러 분기
            bool selected = (i == _currentIndex);
            var bg      = selected ? kSelectedRow : solidButtonBaseColor;
            var txtCol  = selected ? Color.black  : actionBtnText;  // ★ 선택 시 검정!

            GUILayout.BeginHorizontal();

            if (DrawSolidButton("Find", bg, txtCol, GUILayout.Width(50)))
                EditorGUIUtility.PingObject(list[i].Prefab);

            if (DrawSolidButton(list[i].Prefab.name, bg, txtCol, GUILayout.ExpandWidth(true)))
            {
                _currentIndex = i;
                _needsScrollUpdate = true;
                if (Application.isPlaying) PlayCurrent();
            }

            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        if (Event.current.type == EventType.Repaint)
            _scrollViewHeight = GUILayoutUtility.GetLastRect().height;

        // 스킨 복구
        PopSolidSkin();

        DrawResizeHandles();
        GUI.DragWindow(new Rect(0, 0, _windowRect.width, 20));
    }

    private void DrawCategoryButtons()
    {
        GUILayout.BeginHorizontal();

        bool selWorld = _viewMode == ViewMode.World;
        bool selUI = _viewMode == ViewMode.UI;

        if (DrawSolidButton($"World Effect ({_worldFx?.Count ?? 0})",
            selWorld ? tabSelectedBg : tabUnselectedBg,
            selWorld ? tabSelectedText : tabUnselectedText,
            GUILayout.ExpandWidth(true)))
        { _viewMode = ViewMode.World; }

        if (DrawSolidButton($"UI Effect ({_uiFx?.Count ?? 0})",
            selUI ? tabSelectedBg : tabUnselectedBg,
            selUI ? tabSelectedText : tabUnselectedText,
            GUILayout.ExpandWidth(true)))
        { _viewMode = ViewMode.UI; }

        GUILayout.EndHorizontal();

        if (selWorld != (_viewMode == ViewMode.World))
        {
            _currentIndex = 0; _scrollPosition = Vector2.zero;
            _needsScrollUpdate = true; SaveSettings();
        }
    }

    private List<FXPrefabInfo> GetActiveList()
        => (_viewMode == ViewMode.UI) ? (_uiFx ?? new List<FXPrefabInfo>())
                                      : (_worldFx ?? new List<FXPrefabInfo>());

    // ==================== 크기 조절/네비 ====================

    private void DrawResizeHandles()
    {
        var tl = new Rect(0, 0, ResizeHandleSize, ResizeHandleSize);
        var tr = new Rect(_windowRect.width - ResizeHandleSize, 0, ResizeHandleSize, ResizeHandleSize);
        var bl = new Rect(0, _windowRect.height - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);
        var br = new Rect(_windowRect.width - ResizeHandleSize, _windowRect.height - ResizeHandleSize, ResizeHandleSize, ResizeHandleSize);

        EditorGUIUtility.AddCursorRect(tl, MouseCursor.ResizeUpLeft);
        EditorGUIUtility.AddCursorRect(tr, MouseCursor.ResizeUpRight);
        EditorGUIUtility.AddCursorRect(bl, MouseCursor.ResizeUpRight);
        EditorGUIUtility.AddCursorRect(br, MouseCursor.ResizeUpLeft);

        Event e = Event.current;
        if (e.type == EventType.MouseDown)
        {
            if (br.Contains(e.mousePosition)) _activeResizeCorner = ResizeCorner.BottomRight;
            else if (bl.Contains(e.mousePosition)) _activeResizeCorner = ResizeCorner.BottomLeft;
            else if (tr.Contains(e.mousePosition)) _activeResizeCorner = ResizeCorner.TopRight;
            else if (tl.Contains(e.mousePosition)) _activeResizeCorner = ResizeCorner.TopLeft;
            else return;
            e.Use();
        }
    }

    private void HandleGlobalResizeInput()
    {
        if (_activeResizeCorner == ResizeCorner.None) return;

        Event e = Event.current;
        if (e.type == EventType.MouseDrag)
        {
            const float minW = 200, minH = 150;
            switch (_activeResizeCorner)
            {
                case ResizeCorner.BottomRight:
                    _windowRect.width = Mathf.Max(minW, _windowRect.width + e.delta.x);
                    _windowRect.height = Mathf.Max(minH, _windowRect.height + e.delta.y);
                    break;
                case ResizeCorner.BottomLeft:
                    _windowRect.x += e.delta.x;
                    _windowRect.width = Mathf.Max(minW, _windowRect.width - e.delta.x);
                    _windowRect.height = Mathf.Max(minH, _windowRect.height + e.delta.y);
                    break;
                case ResizeCorner.TopRight:
                    _windowRect.y += e.delta.y;
                    _windowRect.width = Mathf.Max(minW, _windowRect.width + e.delta.x);
                    _windowRect.height = Mathf.Max(minH, _windowRect.height - e.delta.y);
                    break;
                case ResizeCorner.TopLeft:
                    _windowRect.x += e.delta.x; _windowRect.y += e.delta.y;
                    _windowRect.width = Mathf.Max(minW, _windowRect.width - e.delta.x);
                    _windowRect.height = Mathf.Max(minH, _windowRect.height - e.delta.y);
                    break;
            }
            e.Use();
        }
        else if (e.rawType == EventType.MouseUp)
        {
            _activeResizeCorner = ResizeCorner.None;
            SaveSettings();
        }
    }

    private void GoToPrevious()
    {
        var list = GetActiveList();
        if (list.Count == 0) return;
        _currentIndex--; if (_currentIndex < 0) _currentIndex = list.Count - 1;
        _needsScrollUpdate = true;
    }
    private void GoToNext()
    {
        var list = GetActiveList();
        if (list.Count == 0) return;
        _currentIndex++; if (_currentIndex >= list.Count) _currentIndex = 0;
        _needsScrollUpdate = true;
    }
    private void GoToPreviousAndPlay() { GoToPrevious(); PlayCurrent(); }
    private void GoToNextAndPlay() { GoToNext(); PlayCurrent(); }

    // ==================== FX 판별/재생 로직(변경 없음) ====================

    private bool PrefabHasUIParticle(GameObject prefab)
    {
        if (prefab == null) return false;
        var comps = prefab.GetComponentsInChildren<Component>(true);
        for (int i = 0; i < comps.Length; i++)
        {
            var c = comps[i]; if (c == null) continue;
            if (c.GetType().Name.Contains("UIParticle")) return true;
        }
        return false;
    }

    private void PlayInstanceNextFrame(GameObject go)
    {
        EditorApplication.delayCall += () =>
        {
            if (go == null) return;
            var uiParticles = go.GetComponentsInChildren<Component>(true)
                                .Where(c => c != null && c.GetType().Name.Contains("UIParticle"));
            foreach (var comp in uiParticles)
            {
                var m = comp.GetType().GetMethod("Refresh",
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (m != null) { try { m.Invoke(comp, null); } catch { } }
            }

            var psList = go.GetComponentsInChildren<ParticleSystem>(true);
            foreach (var ps in psList)
            {
                if (ps == null) continue;
                ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                ps.Clear(true);
                ps.Play(true);
            }
        };
    }

    private void EnsureActive(GameObject go)
    {
        if (go == null) return;
        if (!go.activeSelf) go.SetActive(true);
        var all = go.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            var g = all[i].gameObject;
            if (!g.activeSelf) g.SetActive(true);
        }
    }

    private void ForceStartRotationZero(GameObject go)
    {
        if (go == null) return;
        var psList = go.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in psList)
        {
            var main = ps.main;
            if (main.startRotation3D)
            {
                main.startRotationX = new ParticleSystem.MinMaxCurve(0f);
                main.startRotationY = new ParticleSystem.MinMaxCurve(0f);
                main.startRotationZ = new ParticleSystem.MinMaxCurve(0f);
            }
            else
            {
                main.startRotation = new ParticleSystem.MinMaxCurve(0f);
            }
        }
    }

    private void ForceTransformRotationZero(GameObject go)
    {
        if (go == null) return;
        var trs = go.GetComponentsInChildren<Transform>(true);
        foreach (var tr in trs)
        {
            if (tr is RectTransform) continue;
            if (tr.localRotation != Quaternion.identity) tr.localRotation = Quaternion.identity;
        }
    }

    private void PlayCurrent()
    {
        var list = GetActiveList();
        if (list.Count == 0) return;

        if (_currentInstance != null) Destroy(_currentInstance);

        var info = list[_currentIndex];
        var prefab = info.Prefab;

        if (_uiFxRoot == null)
        {
            var go = GameObject.Find("FX_UI");
            _uiFxRoot = go != null ? go.transform : null;
        }

        if (info.IsUiFx && _uiFxRoot != null)
        {
            _currentInstance = Instantiate(prefab, _uiFxRoot, false);
            EnsureActive(_currentInstance);

            var t = _currentInstance.transform;
            var rt = t as RectTransform ?? _currentInstance.GetComponent<RectTransform>();
            if (rt != null)
            {
                rt.anchoredPosition = Vector2.zero;
                rt.localScale = Vector3.one;
                rt.localRotation = Quaternion.identity;
                rt.SetAsLastSibling();
            }
            else
            {
                t.localPosition = Vector3.zero;
                t.localRotation = Quaternion.identity;
                t.localScale = Vector3.one;
            }

            if (forceZeroTransformRotation) ForceTransformRotationZero(_currentInstance);
            if (forceZeroStartRotation)     ForceStartRotationZero(_currentInstance);
            PlayInstanceNextFrame(_currentInstance);
        }
        else
        {
            _currentInstance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            EnsureActive(_currentInstance);

            if (forceZeroTransformRotation) ForceTransformRotationZero(_currentInstance);
            if (forceZeroStartRotation)     ForceStartRotationZero(_currentInstance);
            PlayInstanceNextFrame(_currentInstance);
        }
    }
}
#endif
