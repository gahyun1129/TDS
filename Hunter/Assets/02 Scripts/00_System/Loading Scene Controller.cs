using System.Collections;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneController : MonoBehaviour
{
    [Header("UI 연결")]
    public Slider progressBar;
    public TextMeshProUGUI progressText;

    private void Start()
    {
        // 시작하자마자 비동기 로딩 프로세스 시작
        LoadGameSceneProcess().Forget();
    }

    private async UniTaskVoid LoadGameSceneProcess()
    {
        // 1. SceneChanger에 저장된 목표 씬 이름을 가져옴
        string targetScene = SceneChanger.Instance.TargetSceneName;

        // 예외처리: 혹시 로딩씬을 바로 실행해서 타겟이 없다면?
        if (string.IsNullOrEmpty(targetScene)) targetScene = "GameScene"; 

        // 2. 비동기 로드 시작 (화면 전환은 막아둠)
        AsyncOperation op = SceneManager.LoadSceneAsync(targetScene);
        op.allowSceneActivation = false;

        float timer = 0f;

        // 3. 로딩 게이지 채우기
        // op.progress는 0.9가 만땅입니다.
        while (!op.isDone)
        {
            await UniTask.Yield(); // 1프레임 대기

            timer += Time.deltaTime;

            if (op.progress < 0.9f)
            {
                // 실제 로딩 중: Lerp로 부드럽게
                progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
                if (progressText) progressText.text = $"{(progressBar.value * 100):F0}%";
            }
            else
            {
                // 로딩은 끝났음(0.9). 이제 UI를 100%까지 예쁘게 채워줌
                progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);
                if (progressText) progressText.text = $"{(progressBar.value * 100):F0}%";

                // 바가 거의 다 찼으면 (0.99 이상)
                if (progressBar.value >= 0.99f)
                {
                    // UI 완벽히 100% 찍고
                    progressBar.value = 1f;
                    if (progressText) progressText.text = "100%";

                    // 4. SceneChanger에게 "나 준비 다 됐어, 넘겨줘" 요청
                    // await를 해서 페이드 아웃 -> 씬 전환 -> 페이드 인이 끝날 때까지 여기서 대기하지 않아도 됨
                    await SceneChanger.Instance.CallTransitionToGame(op);
                    
                    // 이 스크립트는 씬이 바뀌면서 여기서 파괴됨
                    break;
                }
            }
        }
    }
}