using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialController : MonoBehaviour
{

    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemy;
    [SerializeField] private GameObject tutorialPlayer;
    [SerializeField] private GameObject turotialCanvas;

    [Header("애니메이션 튜닝")]
    [SerializeField] private Image transitionImage;
    [SerializeField] private float maxRadius = 1.5f;
    [SerializeField] private float minRadius = -0.3f;

    [Header("Tutorial Components")]
    [SerializeField] private Animator playerAnimator;      // 플레이어 애니메이터
    [SerializeField] private Image handIcon;               // 손 모양 아이콘 이미지
    [SerializeField] private RectTransform handIconRect;   // 손 모양 아이콘의 트랜스폼 (위치 이동용)

    [Header("Settings")]
    [SerializeField] private float dragDistance = 100f;    // 드래그 거리
    [SerializeField] private string moveAnimName = "IsMoving"; // 애니메이터 파라미터 이름 (Bool)
    [SerializeField] private string dragAnimName = "IsDragging"; // 애니메이터 파라미터 이름 (Bool)

    private Material transitionMat;

    void Start()
    {
        transitionMat = transitionImage.material;
        transitionMat.SetFloat("_Radius", maxRadius);
        transitionImage.enabled = false;
    }

    public void StartTutorial()
    {
        FadeOut();
    }

    public void FadeOut()
    {
        transitionImage.enabled = true;
        transitionMat.SetFloat("_Radius", maxRadius);
        transitionMat.DOFloat(minRadius, "_Radius", 0.8f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                player.SetActive(false);
                enemy.SetActive(false);
                FadeIn();
            });
    }

    public void FadeIn()
    {
        tutorialPlayer.SetActive(true);
        CameraController.Instance.ShowTutorialCamera();

        transitionImage.enabled = true;
        transitionMat.SetFloat("_Radius", minRadius);
        transitionMat.DOFloat(maxRadius, "_Radius", 0.8f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                transitionImage.enabled = false;
                ShowTutorial().Forget();
            });


    }

    public InsertController insertController;
    public void GameStart()
    {

        ResetTutorialState();

        showTutorial = false;
        transitionImage.enabled = true;
        transitionMat.SetFloat("_Radius", maxRadius);
        transitionMat.DOFloat(minRadius, "_Radius", 0.8f)
            .SetEase(Ease.InOutQuad)
            .OnComplete(() =>
            {
                CameraController.Instance.ShowMainCamera();
                player.SetActive(true);
                enemy.SetActive(true);
                tutorialPlayer.SetActive(false);
                insertController.AfterTutorial();
                transitionImage.enabled = false;
                turotialCanvas.SetActive(false);
            })
            .SetLink(turotialCanvas);
    }

    [SerializeField] private Image rightBackground;
    [SerializeField] private Image leftBackGround;

    [SerializeField] private TextMeshProUGUI mentText;

    private static string moveMent = "<color=#FFB239>Tap Right</color>\nto move back";
    private static string dragMent = "<color=#FFB239>Drag Left</color>\nto throw";

    [SerializeField] private GameObject PlayButton;
    [SerializeField] private InfiniteBackground infiniteBackground;
    private bool showTutorial = true;

    public async UniTaskVoid ShowTutorial()
    {
        Tween bgTween = null;
        Tween textScaleTween = null;
        Tween handTween = null;

        // -----------------------------------------------------------
        // [위치 계산 로직] Loop 진입 전 미리 계산
        // -----------------------------------------------------------

        // 1. 캔버스 RectTransform 가져오기
        RectTransform canvasRect = turotialCanvas.GetComponent<RectTransform>();

        // 2. 캔버스 렌더 모드 확인 (Overlay면 카메라는 null, Camera면 MainCamera 필요)
        Canvas canvasParams = turotialCanvas.GetComponent<Canvas>();
        Camera uiCamera = (canvasParams.renderMode == RenderMode.ScreenSpaceOverlay) ? null : Camera.main;

        // 3. 뷰포트 좌표 설정 (0.75, 0.5 / 0.25, 0.5)
        Vector2 viewportMovePos = new Vector2(0.75f, 0.3f);
        Vector2 viewportAimPos = new Vector2(0.25f, 0.3f);

        // 4. 좌표 변환 (Viewport -> Screen -> Canvas Local)
        Vector2 moveHandPos; // 이동 튜토리얼용 손 위치
        Vector2 aimHandPos;  // 에임 튜토리얼용 손 위치

        // 이동용 위치 변환
        Vector2 screenPosRight = new Vector2(Screen.width * viewportMovePos.x, Screen.height * viewportMovePos.y);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosRight, uiCamera, out moveHandPos);

        // 에임용 위치 변환
        Vector2 screenPosLeft = new Vector2(Screen.width * viewportAimPos.x, Screen.height * viewportAimPos.y);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPosLeft, uiCamera, out aimHandPos);

        // -----------------------------------------------------------

        ResetTutorialState();

        await UniTask.Delay(500);

        while (showTutorial)
        {
            // =================================================================
            // PHASE 1: 이동 튜토리얼 (오른쪽)
            // =================================================================
            rightBackground.gameObject.SetActive(true);
            mentText.gameObject.SetActive(true);
            mentText.text = moveMent;

            bgTween = rightBackground.DOFade(0f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            textScaleTween = mentText.transform.DOScale(1.1f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

            // 배경 레이어 설정 (예: 3번 레이어만 이동)
            int[] tutorialLayers = new int[] { 3 };
            float scrollSpeed = 1f;

            // 초기화
            handIcon.color = Color.white;
            handIcon.transform.localScale = Vector3.one;

            // [위치 적용] 미리 계산해둔 이동용 위치로 설정
            handIconRect.anchoredPosition = moveHandPos;

            handTween = DOTween.Sequence()
                // 1. [시작] 켜기 & 투명도 리셋 & 이동 시작 & 배경 스크롤 시작
                .AppendCallback(() => {
                    handIcon.gameObject.SetActive(true);
                    handIcon.color = Color.white;
                    handIconRect.anchoredPosition = moveHandPos; // 위치 확실히 고정

                    PlayerMoving();

                    infiniteBackground.StartTutorialScroll(tutorialLayers, scrollSpeed);
                })
                // 2. 꾹 누르기
                .Append(handIcon.transform.DOScale(0.8f, 0.2f).SetEase(Ease.OutQuad))
                // 3. 유지
                .AppendInterval(1.5f)
                // 4. 손 떼기
                .Append(handIcon.transform.DOScale(1f, 0.15f).SetEase(Ease.OutQuad))
                // 5. 사라지기
                .Append(handIcon.DOFade(0f, 0.3f))
                // 6. 끄기 및 정리
                .AppendCallback(() => {
                    PlayerIdle();
                    handIcon.gameObject.SetActive(false);
                    infiniteBackground.StopTutorialScroll();
                })
                // 7. 대기
                .AppendInterval(1f)
                .SetLoops(-1);

            rightBackground.color = new Vector4(1, 1, 1, 15 / 255f);
            await UniTask.Delay(5000);

            // --- 정리 (Phase 1) ---
            infiniteBackground.StopTutorialScroll();

            rightBackground.gameObject.SetActive(false);
            mentText.gameObject.SetActive(false);

            bgTween?.Kill();
            textScaleTween?.Kill();
            handTween?.Kill();

            PlayerIdle();
            handIcon.gameObject.SetActive(false);
            handIcon.transform.localScale = Vector3.one;

            if (!showTutorial) break;
            await UniTask.Delay(500);
            if (!showTutorial) break;


            // =================================================================
            // PHASE 2: 에임/드래그 튜토리얼 (왼쪽)
            // =================================================================
            leftBackGround.gameObject.SetActive(true);
            mentText.gameObject.SetActive(true);
            mentText.text = dragMent;

            bgTween = leftBackGround.DOFade(0f, 0.6f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);
            textScaleTween = mentText.transform.DOScale(1.1f, 0.4f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.Linear);

            handIcon.gameObject.SetActive(true);
            handIcon.color = Color.white;

            // [위치 적용] 미리 계산해둔 에임용 위치를 시작점으로 사용
            Vector2 startPos = aimHandPos;
            Vector2 targetPos = startPos + new Vector2(-dragDistance, -dragDistance);

            // 시작 위치로 이동
            handIconRect.anchoredPosition = startPos;

            handTween = DOTween.Sequence()
                .AppendCallback(() => PlayerAiming())
                .Append(handIconRect.DOAnchorPos(targetPos, 1f).SetEase(Ease.OutQuad))
                .Join(handIcon.DOFade(0f, 0.3f).SetDelay(0.7f))
                .AppendCallback(() => PlayerIdle())
                .AppendCallback(() => {
                    handIconRect.anchoredPosition = startPos; // 다시 aimHandPos로 복구
                    handIcon.color = Color.white;
                })
                .AppendInterval(0.5f)
                .SetLoops(-1);

            await UniTask.Delay(5000);

            // --- 정리 (Phase 2) ---
            leftBackGround.gameObject.SetActive(false);
            mentText.gameObject.SetActive(false);

            bgTween?.Kill();
            textScaleTween?.Kill();
            handTween?.Kill();

            PlayerIdle();
            handIconRect.anchoredPosition = startPos;
            handIcon.gameObject.SetActive(false);
            mentText.transform.localScale = Vector3.one;
            leftBackGround.color = new Vector4(1, 1, 1, 15 / 255f);
            PlayButton.SetActive(true);
        }

        ResetTutorialState();
    }

    private void PlayerMoving()
    {
        playerAnimator.speed = 1.3f;
        playerAnimator.SetBool(BearController.PLAYER_IDLE, false);
        playerAnimator.SetBool(BearController.PLAYER_MOVING, true);
    }

    private void PlayerAiming()
    {
        playerAnimator.SetBool(BearController.PLAYER_IDLE, false);
        playerAnimator.SetBool(BearController.PLAYER_AIMING, true);
    }

    private void PlayerIdle()
    {
        playerAnimator.speed = 1f;
        playerAnimator.SetBool(BearController.PLAYER_IDLE, true);
        playerAnimator.SetBool(BearController.PLAYER_AIMING, false);
        playerAnimator.SetBool(BearController.PLAYER_MOVING, false);
    }
    private void ResetTutorialState()
    {
        // 모든 UI 끄기
        rightBackground.gameObject.SetActive(false);
        leftBackGround.gameObject.SetActive(false);
        mentText.gameObject.SetActive(false);
        handIcon.gameObject.SetActive(false);

        rightBackground.color = new Vector4(1, 1, 1, 15/255f);
        leftBackGround.color = new Vector4(1, 1, 1, 15/255f);
        // 애니메이터 초기화 (모든 파라미터 끄기)
        if (playerAnimator != null)
        {
            playerAnimator.SetBool(BearController.PLAYER_MOVING, false);
            playerAnimator.SetBool(BearController.PLAYER_AIMING, false);
            playerAnimator.SetBool(BearController.PLAYER_IDLE, true);
        }

        // 텍스트 스케일 복구
        mentText.transform.localScale = Vector3.one;
        infiniteBackground.StopTutorialScroll();
    }

}
