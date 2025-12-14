using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class InsertController : MonoBehaviour
{
    [Header("Objects")]
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject enemy;
    [SerializeField] private Animator playerAnimator;
    [SerializeField] private Animator enemyAnimator;

    [Header("Game Start")]
    [SerializeField] private GameObject InsertPopup;
    [SerializeField] private GameObject skipButton;

    [SerializeField] private TutorialController tutorialController;
    private LevelData levelData;
    private CancellationTokenSource _insertCts;

    void Start()
    {
        levelData = GameDataManager.Instance.currentLevelData;

        if (levelData.stageNum == 1)
        {
            StartInsert().Forget();
            skipButton.SetActive(true);
        }
        else
        {
            Vector3 playerFinalPos = player.transform.position;
            playerFinalPos.x = 0;
            player.transform.position = playerFinalPos;
            playerAnimator.speed = 1f;

            CameraController.Instance.ShowMainCamera();
            enemyAnimator.SetTrigger("Moving");

            float targetX = levelData.enemyBase.stopWorldDistance;
            Vector3 targetPosition = new Vector3(targetX, enemy.transform.position.y, enemy.transform.position.z);

            enemy.transform.DOMove(targetPosition, 2f).SetEase(Ease.Linear)
                            .OnComplete(() => { enemyAnimator.Play("Idle"); });

            OnGameStart();
        }
    }

    public void OnTutorialStart()
    {
        skipButton.SetActive(false);
        tutorialController.StartTutorial();
    }

    public void AfterTutorial()
    {
        InsertPopup.SetActive(true);
        skipButton.SetActive(false);

        enemyAnimator.ResetTrigger("Moving");
        enemyAnimator.ResetTrigger("Idle");
        enemyAnimator.speed = 1f;
        enemyAnimator.Play("Idle", 0, 0f);

        BearController bearController = player.GetComponent<BearController>();
        if (bearController)
        {
            bearController.enabled = true;
            bearController.ResetState();
        }

        if (enemy.GetComponent<EnemyController>())
            enemy.GetComponent<EnemyController>().enabled = true;
    }
    
    
    public void OnGameStart()
    {
        if (levelData.stageNum == 1)
        {
            OnTutorialStart();
            return;
        }
        
        InsertPopup.SetActive(true);
        skipButton.SetActive(false);
        
        enemyAnimator.ResetTrigger("Moving");
        enemyAnimator.ResetTrigger("Idle");
        enemyAnimator.speed = 1f;
        enemyAnimator.Play("Idle", 0, 0f);

        BearController bearController = player.GetComponent<BearController>();
        if (bearController)
        {
            bearController.enabled = true;
            bearController.ResetState();
        }
        
        if (enemy.GetComponent<EnemyController>())
            enemy.GetComponent<EnemyController>().enabled = true;
    }
    

    public void SkipInsert()
    {
        if (_insertCts != null && !_insertCts.IsCancellationRequested)
        {
            var tempCts = _insertCts;

            _insertCts = null;

            tempCts.Cancel();
            tempCts.Dispose();
        }

        enemy.transform.DOKill();
        player.transform.DOKill();

        Vector3 playerFinalPos = player.transform.position;
        playerFinalPos.x = 0;
        player.transform.position = playerFinalPos;

        CameraController.Instance.ShowMainCamera();

        playerAnimator.speed = 1f;

        float targetX = levelData.enemyBase.stopWorldDistance;
        Vector3 enemyFinalPos = new Vector3(targetX, enemy.transform.position.y, enemy.transform.position.z);
        enemy.transform.position = enemyFinalPos;

        OnGameStart();
    }

    public async UniTaskVoid StartInsert()
    {
        // 새로운 CancellationTokenSource 생성
        _insertCts = new CancellationTokenSource();

        try
        {
            await UniTask.Delay(1000, cancellationToken: _insertCts.Token);

            CameraController.Instance.Shake(0.1f, 1f);

            await UniTask.Delay(500, cancellationToken: _insertCts.Token);

            CameraController.Instance.ShowEnemyCamera();

            await UniTask.Delay(1000, cancellationToken: _insertCts.Token);

            CameraController.Instance.ShowDogCamera();

            PlayerMove(_insertCts.Token).Forget();

            await UniTask.Delay(2000, cancellationToken: _insertCts.Token);

            CameraController.Instance.ShowEnemyCamera();
            enemyAnimator.SetTrigger("Moving");

            // 플레이어 위치 기준으로 stopWorldDistance만큼 오른쪽 위치 계산
            float targetX = levelData.enemyBase.stopWorldDistance;
            Vector3 targetPosition = new Vector3(targetX, enemy.transform.position.y, enemy.transform.position.z);


            skipButton.SetActive(false);
            
            // DOTween으로 이동
            await enemy.transform.DOMove(targetPosition, 2f).SetEase(Ease.Linear)
                .ToUniTask(cancellationToken: _insertCts.Token);

            enemyAnimator.SetTrigger("Idle");

            Vector3 finalPos = player.transform.position;
            finalPos.x = 0;
            player.transform.position = finalPos;

            await UniTask.Delay(200, cancellationToken: _insertCts.Token);
                        
            CameraController.Instance.ShowMainCamera();

            await UniTask.Delay(200, cancellationToken: _insertCts.Token);

            OnGameStart();

        }
        catch (System.OperationCanceledException)
        {
            Debug.Log("Insert cancelled");
        }
        finally
        {
            skipButton.SetActive(false);
            _insertCts?.Dispose();
            _insertCts = null;
        }
    }

    public async UniTaskVoid PlayerMove(CancellationToken cancellationToken)
    {
        try
        {
            playerAnimator.SetBool(BearController.PLAYER_IDLE, false);
            playerAnimator.SetBool(BearController.PLAYER_MOVING, true);

            float moveSpeed = 1.5f;
            playerAnimator.speed = moveSpeed;

            while (player.transform.position.x > 0)
            {
                player.transform.Translate(Vector3.left * moveSpeed * Time.deltaTime);
                await UniTask.Yield(cancellationToken);
            }

            playerAnimator.speed = 1f;

            playerAnimator.SetBool(BearController.PLAYER_MOVING, false);
            playerAnimator.SetBool(BearController.PLAYER_IDLE, true);

            Vector3 finalPos = player.transform.position;
            finalPos.x = 0;
            player.transform.position = finalPos;
        }
        catch (System.OperationCanceledException)
        {
            playerAnimator.speed = 1f;
        }
    }



}
