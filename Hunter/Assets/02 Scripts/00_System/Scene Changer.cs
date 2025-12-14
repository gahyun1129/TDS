using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneChanger : MonoBehaviour
{
    public static SceneChanger Instance { get; private set; }

    [Header("Settings")]
    public Image transitionImage; 
    public float transitionDuration = 1.0f;
    [SerializeField] private string loadingSceneName = "Loading Scene"; 

    [Header("Touch Blocker")]
    public GameObject touchBlocker; 

    public string TargetSceneName { get; private set; }

    private int radiusID;
    private Material materialInstance;
    private bool isTransitioning = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        radiusID = Shader.PropertyToID("_Radius");
        materialInstance = transitionImage.material;
    }

    private void Start()
    {
        materialInstance.SetFloat(radiusID, -0.2f);
        transitionImage.enabled = false;
        if (touchBlocker) touchBlocker.SetActive(false);
    }

    // ========================================
    // 1. 페이드 아웃 + 로딩 씬 + 페이드 인 => 씬 변경
    // ========================================
    public void LoadSceneWithLoading(string sceneName)
    {
        if (isTransitioning) return;
        TargetSceneName = sceneName;
        ProcessSceneChangeWithLoading().Forget();
    }

    private async UniTaskVoid ProcessSceneChangeWithLoading()
    {
        isTransitioning = true;
        if (touchBlocker) touchBlocker.SetActive(true);

        // A. 페이드 아웃 (화면 깜깜해짐)
        await TransitionEffect(true);

        // B. 로딩 씬 로드
        await SceneManager.LoadSceneAsync(loadingSceneName);

        // 화면은 계속 검정색(쉐이더 닫힌 상태) 유지됨.
        // LoadingSceneController가 CallTransitionToGame을 호출할 때까지 대기
    }

    // LoadingSceneController가 호출하는 마무리 프로세스
    public async UniTask CallTransitionToGame(AsyncOperation op)
    {
        // C. 씬 전환 허용
        op.allowSceneActivation = true;

        // 씬이 완전히 바뀔 때까지 대기
        await UniTask.WaitUntil(() => op.isDone);

        // D. 페이드 인 (인게임 화면 보여주기)
        await TransitionEffect(false);

        if (touchBlocker) touchBlocker.SetActive(false);
        isTransitioning = false;
    }

    // ========================================
    // 2. 페이드 아웃 + 페이드 인 => 씬 변경
    // ========================================
    public void LoadSceneWithFade(string sceneName)
    {
        if (isTransitioning) return;
        ProcessSceneChangeWithFade(sceneName).Forget();
    }

    private async UniTaskVoid ProcessSceneChangeWithFade(string sceneName)
    {
        isTransitioning = true;
        if (touchBlocker) touchBlocker.SetActive(true);

        // A. 페이드 아웃
        await TransitionEffect(true);

        // B. 씬 로드
        await SceneManager.LoadSceneAsync(sceneName);

        // C. 페이드 인
        await TransitionEffect(false);

        if (touchBlocker) touchBlocker.SetActive(false);
        isTransitioning = false;
    }

    // ========================================
    // 3. 그냥 씬 변경
    // ========================================
    public void LoadSceneImmediate(string sceneName)
    {
        if (isTransitioning) return;
        ProcessSceneChangeImmediate(sceneName).Forget();
    }

    private async UniTaskVoid ProcessSceneChangeImmediate(string sceneName)
    {
        isTransitioning = true;
        if (touchBlocker) touchBlocker.SetActive(true);

        // 씬 로드만
        await SceneManager.LoadSceneAsync(sceneName);

        if (touchBlocker) touchBlocker.SetActive(false);
        isTransitioning = false;
    }

    // ========================================
    // 4. 페이드 인 => 씬 변경
    // ========================================
    public void LoadSceneWithFadeIn(string sceneName)
    {
        if (isTransitioning) return;
        ProcessSceneChangeWithFadeIn(sceneName).Forget();
    }

    private async UniTaskVoid ProcessSceneChangeWithFadeIn(string sceneName)
    {
        isTransitioning = true;
        if (touchBlocker) touchBlocker.SetActive(true);

        // A. 씬 로드
        await SceneManager.LoadSceneAsync(sceneName);

        // B. 페이드 인
        await TransitionEffect(false);

        if (touchBlocker) touchBlocker.SetActive(false);
        isTransitioning = false;
    }

    // ========================================
    // 5. 페이드 아웃 => 씬 변경
    // ========================================
    public void LoadSceneWithFadeOut(string sceneName)
    {
        if (isTransitioning) return;
        ProcessSceneChangeWithFadeOut(sceneName).Forget();
    }

    private async UniTaskVoid ProcessSceneChangeWithFadeOut(string sceneName)
    {
        isTransitioning = true;
        if (touchBlocker) touchBlocker.SetActive(true);

        // A. 페이드 아웃
        await TransitionEffect(true);

        // B. 씬 로드
        await SceneManager.LoadSceneAsync(sceneName);

        if (touchBlocker) touchBlocker.SetActive(false);
        isTransitioning = false;
    }

    // ========================================
    // [하위 호환성] 기존 메서드 유지
    // ========================================
    public void LoadScene(string sceneName)
    {
        LoadSceneWithLoading(sceneName);
    }

    private async UniTask TransitionEffect(bool isClose)
    {
        transitionImage.enabled = true;
        float timer = 0f;

        float startValue = isClose ? 1.2f : -0.5f;
        float endValue = isClose ? -0.5f : 1.2f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float t = Mathf.SmoothStep(startValue, endValue, timer / transitionDuration);
            materialInstance.SetFloat(radiusID, t);
            await UniTask.Yield();
        }

        materialInstance.SetFloat(radiusID, endValue);
        
        if (!isClose) 
        {
            transitionImage.enabled = false;
        }
    }
}
